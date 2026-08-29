using EFfluentify.Domain.Helpers;
using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Helpers;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class ForeignKeyFluentRule : EntityFluentRuleBase
    {
        private readonly ModelContext _ctx;

        public ForeignKeyFluentRule(ModelContext ctx)
        {
            _ctx = ctx;
        }

        protected override IEnumerable<string> SupportedAttributeNames => new[] { "ForeignKey", "InverseProperty", "DeleteBehavior" };

        public override bool CanApply(EntityModel entity)
            => entity.Properties.Any(EfTypeHelper.HasForeignKeyAttribute);

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var pairs = CollectForeignKeyPairs(entity).ToList();

            foreach (var pair in pairs)
            {
                foreach (var line in BuildRelationshipLines(entity, pair))
                    yield return line;
            }
        }

        private sealed record ForeignKeyPair(
            string NavName,
            string PrincipalTypeName,
            IReadOnlyList<string> FkPropertyNames,
            string? InverseNavName
        );

        private IEnumerable<ForeignKeyPair> CollectForeignKeyPairs(EntityModel entity)
        {
            foreach (var prop in entity.Properties)
            {
                var fkAttr = EfTypeHelper.TryGetForeignKeyAttribute(prop);
                if (fkAttr == null)
                    continue;

                var arg0 = fkAttr.PositionalArgs.FirstOrDefault() as string;
                if (string.IsNullOrWhiteSpace(arg0))
                    continue;

                if (IsNavigationProperty(entity, prop))
                {
                    var fkProps = EfTypeHelper.SplitFkNames(arg0).ToList();
                    if (fkProps.Count == 0)
                        continue;

                    yield return new ForeignKeyPair(
                        NavName: prop.Name,
                        PrincipalTypeName: prop.TypeName,
                        FkPropertyNames: fkProps,
                        InverseNavName: GetInversePropertyName(prop)
                    );
                }
                else
                {
                    var navName = EfTypeHelper.NormalizeMemberName(arg0);
                    if (string.IsNullOrWhiteSpace(navName))
                        continue;

                    var navProp = entity.Properties.FirstOrDefault(p => p.Name == navName);

                    yield return new ForeignKeyPair(
                        NavName: navName!,
                        PrincipalTypeName: navProp?.TypeName ?? "",
                        FkPropertyNames: new[] { prop.Name },
                        InverseNavName: navProp != null ? GetInversePropertyName(navProp) : null
                    );
                }
            }
        }

        private IEnumerable<string> BuildRelationshipLines(EntityModel dependentEntity, ForeignKeyPair pair)
        {
            // Determine principal type
            var principalType = EfTypeHelper.NormalizeTypeName(pair.PrincipalTypeName);
            if (string.IsNullOrWhiteSpace(principalType))
            {
                // Try fallback: infer from nav property on dependent entity
                var navProp = dependentEntity.Properties.FirstOrDefault(p => p.Name == pair.NavName);
                if (navProp != null)
                    principalType = EfTypeHelper.NormalizeTypeName(navProp.TypeName);
            }

            // choose WithMany vs WithOne (+ inverse nav if can find)
            var (withCall, isOneToOne) = DecideInverse(dependentEntity, pair);

            var fkExpr = pair.FkPropertyNames.Count == 1
                ? $"x => x.{pair.FkPropertyNames[0]}"
                : $"x => new {{ {string.Join(", ", pair.FkPropertyNames.Select(n => $"x.{n}"))} }}";

            var sb = new System.Text.StringBuilder();
            sb.Append($"builder.HasOne(x => x.{pair.NavName})");
            sb.Append(withCall);

            // One-to-one: often needs generic HasForeignKey<Dependent>
            if (isOneToOne)
            {
                sb.Append($".HasForeignKey<{dependentEntity.Name}>({fkExpr})");
            }
            else
            {
                sb.Append($".HasForeignKey({fkExpr})");
            }

            // Check IsRequired
            if (IsRequired(dependentEntity, pair.FkPropertyNames))
            {
                sb.Append(".IsRequired()");
            }

            var deleteBehavior = GetDeleteBehavior(dependentEntity, pair);
            if (!string.IsNullOrEmpty(deleteBehavior))
            {
                sb.Append($".OnDelete({deleteBehavior})");
            }

            sb.Append(";");
            yield return sb.ToString();
        }

        private (string withCall, bool isOneToOne) DecideInverse(EntityModel dependent, ForeignKeyPair pair)
        {
            // 1. Explicit [InverseProperty] on the Dependent Navigation Property
            if (!string.IsNullOrEmpty(pair.InverseNavName))
            {
                // We have the name of the navigation property on the Principal side.
                // We need to know if it's a Collection or Reference to choose WithMany vs WithOne.

                // If we can resolve principal entity, check the property type
                var principalTypeName = EfTypeHelper.NormalizeTypeName(pair.PrincipalTypeName);
                if (_ctx.TryGetEntity(principalTypeName, out var principal))
                {
                    var invProp = principal.Properties.FirstOrDefault(p => p.Name == pair.InverseNavName);
                    if (invProp != null)
                    {
                        if (EfTypeHelper.IsCollectionType(invProp.TypeName))
                            return ($".WithMany(x => x.{pair.InverseNavName})", false);

                        return ($".WithOne(x => x.{pair.InverseNavName})", true);
                    }
                }

                // Fallback: If we can't find the property on principal (maybe missing source), we guess.
                // Usually matching inverse implies bidirection.
                // If we assume standard 1:N, then inverse is Collection -> WithMany.
                // If we assume 1:1, inverse is Reference -> WithOne.
                // For safety, defaulting to WithMany is common, BUT OneToOneSignal is strong.
                if (EfTypeHelper.HasOneToOneSignal(dependent))
                    return ($".WithOne(x => x.{pair.InverseNavName})", true);

                return ($".WithMany(x => x.{pair.InverseNavName})", false);
            }

            // 2. Resolve Principal and look for [InverseProperty] pointing BACK to us
            var pType = EfTypeHelper.NormalizeTypeName(pair.PrincipalTypeName);
            if (_ctx.TryGetEntity(pType, out var principalEntity))
            {
                // Search for a property on Principal that has [InverseProperty(pair.NavName)]
                var invProp = principalEntity.Properties
                    .FirstOrDefault(p =>
                    {
                        var attr = EfTypeHelper.TryGetInversePropertyAttribute(p);
                        if (attr == null) return false;

                        var arg = EfTypeHelper.NormalizeMemberName(attr.PositionalArgs.FirstOrDefault() as string ?? "");
                        return string.Equals(arg, pair.NavName, StringComparison.Ordinal);
                    });

                if (invProp != null)
                {
                    if (EfTypeHelper.IsCollectionType(invProp.TypeName))
                        return ($".WithMany(x => x.{invProp.Name})", false);

                    return ($".WithOne(x => x.{invProp.Name})", true);
                }

                // 3. Auto-discovery by type matching (existing logic)
                var inverseCandidates = FindInverseNavCandidates(principalEntity, dependent.Name).ToList();

                // Refined Heuristic:
                // If One-to-One signal is present, we expect the inverse to be a Reference property (WithOne).
                // If not, we expect the inverse to be a Collection property (WithMany).

                if (EfTypeHelper.HasOneToOneSignal(dependent))
                {
                    // Look for single Reference candidate
                    var refs = inverseCandidates.Where(c => !c.isCollection).ToList();
                    if (refs.Count == 1)
                        return ($".WithOne(x => x.{refs[0].name})", true);

                    // If ambiguous or none, default
                    return (".WithOne()", true);
                }
                else
                {
                    // Standard heuristic:
                    // If there is exactly one Collection candidate -> WithMany
                    // If there is exactly one Reference candidate -> WithOne (Standard 1:1)
                    var cols = inverseCandidates.Where(c => c.isCollection).ToList();
                    var refs = inverseCandidates.Where(c => !c.isCollection).ToList();

                    if (cols.Count == 1 && refs.Count == 0)
                        return ($".WithMany(x => x.{cols[0].name})", false);

                    if (refs.Count == 1 && cols.Count == 0)
                        return ($".WithOne(x => x.{refs[0].name})", true);

                    // If ambiguous (multiple match), prefer Collection if any exist (1:N is more common than 1:1)
                    // Use the first collection candidate instead of defaulting to .WithMany()
                    if (cols.Count > 0)
                        return ($".WithMany(x => x.{cols[0].name})", false);

                    // If only multiple refs exist...? Rare. Default to WithMany is safer unless we are sure.
                    // But if we have NO candidates, we fall through.
                    return (".WithMany()", false);
                }
            }

            // No principal entity available => fall back to safest default
            if (EfTypeHelper.HasOneToOneSignal(dependent))
                return (".WithOne()", true);

            return (".WithMany()", false);
        }

        private static IEnumerable<(string name, bool isCollection)> FindInverseNavCandidates(EntityModel principal, string dependentEntityName)
        {
            foreach (var p in principal.Properties)
            {
                if (EfTypeHelper.IsCollectionType(p.TypeName))
                {
                    var elem = EfTypeHelper.TryGetCollectionElementType(p.TypeName);
                    if (string.Equals(EfTypeHelper.NormalizeTypeName(elem ?? ""), dependentEntityName, StringComparison.Ordinal))
                        yield return (p.Name, true);
                }
                else
                {
                    var refType = EfTypeHelper.NormalizeTypeName(p.TypeName);
                    if (string.Equals(refType, dependentEntityName, StringComparison.Ordinal))
                        yield return (p.Name, false);
                }
            }
        }

        private static bool IsRequired(EntityModel entity, IEnumerable<string> fkProps)
        {
            // If ANY of the FK properties is nullable, the relationship is optional.
            // If ALL are non-nullable, it is required.
            foreach (var fkName in fkProps)
            {
                var p = entity.Properties.FirstOrDefault(x => x.Name == fkName);
                if (p != null && p.IsNullable)
                    return false;
            }
            return true;
        }

        private static bool IsNavigationProperty(EntityModel entity, Property prop)
        {
            if (EfTypeHelper.IsCollectionType(prop.TypeName))
                return true;

            if (EfTypeHelper.IsScalarType(prop.TypeName))
                return false;

            return true;
        }

        private static string? GetInversePropertyName(Property prop)
        {
            var attr = EfTypeHelper.TryGetInversePropertyAttribute(prop);
            if (attr == null) return null;
            return EfTypeHelper.NormalizeMemberName(attr.PositionalArgs.FirstOrDefault() as string ?? "");
        }

        private static string? GetDeleteBehavior(EntityModel entity, ForeignKeyPair pair)
        {
            // 1. Look on the dependent navigation property
            var navProp = entity.Properties.FirstOrDefault(p => p.Name == pair.NavName);
            if (navProp != null)
            {
                var attr = EfTypeHelper.TryGetDeleteBehaviorAttribute(navProp);
                if (attr != null)
                {
                    var arg = attr.PositionalArgs.FirstOrDefault() ?? attr.NamedArgs.GetValueOrDefault("behavior");
                    if (!string.IsNullOrWhiteSpace(arg))
                        return FormatDeleteBehavior(arg);
                }
            }

            return null;
        }

        private static string FormatDeleteBehavior(string arg)
        {
            // arg comes from Roslyn, likely "DeleteBehavior.Cascade" or "Cascade"
            // We want to ensure it is "DeleteBehavior.Cascade" for the generated code.

            if (arg.Contains("DeleteBehavior."))
                return arg; // Already fully qualified-ish

            // If it's just "Cascade", prepend
            return $"DeleteBehavior.{arg}";
        }
    }
}
