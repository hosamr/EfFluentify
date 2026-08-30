using System.Text;
using EFFluentify.Domain.Helpers;
using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Helpers;

namespace EFFluentify.Domain.Rules.EntityRules
{
    public sealed class ForeignKeyFluentRule : EntityFluentRuleBase
    {
        private readonly ModelContext _ctx;

        public ForeignKeyFluentRule(ModelContext ctx)
        {
            _ctx = ctx;
        }

        protected override IEnumerable<string> SupportedAttributeNames => new[] { "ForeignKey", "InverseProperty", "DeleteBehavior" };

        protected override AttributeScope Scope => AttributeScope.Property;

        public override bool CanApply(EntityModel entity)
            => entity.Properties.Any(EfTypeHelper.HasForeignKeyAttribute);

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var seenNavs = new HashSet<string>(StringComparer.Ordinal);

            foreach (var pair in CollectForeignKeyPairs(entity))
            {
                if (!seenNavs.Add(pair.NavName))
                    continue;

                yield return BuildRelationshipLine(entity, pair);
            }
        }

        private sealed record ForeignKeyPair(
            string NavName,
            string PrincipalTypeName,
            IReadOnlyList<string> FkPropertyNames,
            string? InverseNavName
        );

        private readonly record struct Inverse(string Call, bool IsOneToOne);

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

                var pair = IsNavigationProperty(prop)
                    ? BuildPairFromNavigation(prop, arg0!)
                    : BuildPairFromScalar(entity, prop, arg0!);

                if (pair != null)
                    yield return pair;
            }
        }

        private static ForeignKeyPair? BuildPairFromNavigation(Property navProp, string arg0)
        {
            var fkProps = EfTypeHelper.SplitFkNames(arg0).ToList();
            if (fkProps.Count == 0)
                return null;

            return new ForeignKeyPair(
                NavName: navProp.Name,
                PrincipalTypeName: navProp.TypeName,
                FkPropertyNames: fkProps,
                InverseNavName: GetInversePropertyName(navProp));
        }

        private static ForeignKeyPair? BuildPairFromScalar(EntityModel entity, Property fkProp, string arg0)
        {
            var navName = EfTypeHelper.NormalizeMemberName(arg0);
            if (string.IsNullOrWhiteSpace(navName))
                return null;

            var navProp = entity.Properties.FirstOrDefault(p => p.Name == navName);

            return new ForeignKeyPair(
                NavName: navName!,
                PrincipalTypeName: navProp?.TypeName ?? "",
                FkPropertyNames: new[] { fkProp.Name },
                InverseNavName: navProp != null ? GetInversePropertyName(navProp) : null);
        }

        private string BuildRelationshipLine(EntityModel dependent, ForeignKeyPair pair)
        {
            var inverse = ResolveInverse(dependent, pair);
            var fkExpr = BuildFkLambda(pair.FkPropertyNames);

            var sb = new StringBuilder();
            sb.Append($"builder.HasOne(x => x.{pair.NavName})");
            sb.Append(inverse.Call);

            sb.Append(inverse.IsOneToOne
                ? $".HasForeignKey<{dependent.Name}>({fkExpr})"
                : $".HasForeignKey({fkExpr})");

            if (IsRequired(dependent, pair.FkPropertyNames))
                sb.Append(".IsRequired()");

            var deleteBehavior = GetDeleteBehavior(dependent, pair);
            if (!string.IsNullOrEmpty(deleteBehavior))
                sb.Append($".OnDelete({deleteBehavior})");

            return sb.Append(';').ToString();
        }

        private static string BuildFkLambda(IReadOnlyList<string> fkProps) =>
            fkProps.Count == 1
                ? $"x => x.{fkProps[0]}"
                : $"x => new {{ {string.Join(", ", fkProps.Select(n => $"x.{n}"))} }}";

        private Inverse ResolveInverse(EntityModel dependent, ForeignKeyPair pair)
        {
            if (!string.IsNullOrEmpty(pair.InverseNavName))
                return ResolveExplicitInverse(dependent, pair);

            var principalType = EfTypeHelper.NormalizeTypeName(pair.PrincipalTypeName);
            if (_ctx.TryGetEntity(principalType, out var principal))
            {
                return ResolveFromPrincipalInverseAttribute(principal, pair)
                    ?? ResolveByAutoDiscovery(dependent, principal, pair);
            }

            return DefaultInverse(dependent);
        }

        private Inverse ResolveExplicitInverse(EntityModel dependent, ForeignKeyPair pair)
        {
            var principalType = EfTypeHelper.NormalizeTypeName(pair.PrincipalTypeName);
            if (_ctx.TryGetEntity(principalType, out var principal))
            {
                var invProp = principal.Properties.FirstOrDefault(p => p.Name == pair.InverseNavName);
                if (invProp != null)
                    return InverseFor(invProp.TypeName, pair.InverseNavName!);
            }

            return EfTypeHelper.HasOneToOneSignal(dependent)
                ? new Inverse($".WithOne(x => x.{pair.InverseNavName})", true)
                : new Inverse($".WithMany(x => x.{pair.InverseNavName})", false);
        }

        private static Inverse? ResolveFromPrincipalInverseAttribute(EntityModel principal, ForeignKeyPair pair)
        {
            var invProp = principal.Properties.FirstOrDefault(p =>
            {
                var attr = EfTypeHelper.TryGetInversePropertyAttribute(p);
                if (attr == null)
                    return false;

                var arg = EfTypeHelper.NormalizeMemberName(attr.PositionalArgs.FirstOrDefault() as string ?? "");
                return string.Equals(arg, pair.NavName, StringComparison.Ordinal);
            });

            return invProp == null ? null : InverseFor(invProp.TypeName, invProp.Name);
        }

        private static Inverse ResolveByAutoDiscovery(EntityModel dependent, EntityModel principal, ForeignKeyPair pair)
        {
            var excludeNav = string.Equals(principal.Name, dependent.Name, StringComparison.Ordinal)
                ? pair.NavName
                : null;

            var candidates = FindInverseNavCandidates(principal, dependent.Name, excludeNav).ToList();
            var collections = candidates.Where(c => c.isCollection).ToList();
            var references = candidates.Where(c => !c.isCollection).ToList();

            if (EfTypeHelper.HasOneToOneSignal(dependent))
            {
                return references.Count == 1
                    ? new Inverse($".WithOne(x => x.{references[0].name})", true)
                    : new Inverse(".WithOne()", true);
            }

            if (collections.Count == 1 && references.Count == 0)
                return new Inverse($".WithMany(x => x.{collections[0].name})", false);

            if (references.Count == 1 && collections.Count == 0)
                return new Inverse($".WithOne(x => x.{references[0].name})", true);

            if (collections.Count > 0)
                return new Inverse($".WithMany(x => x.{collections[0].name})", false);

            return new Inverse(".WithMany()", false);
        }

        private static Inverse DefaultInverse(EntityModel dependent) =>
            EfTypeHelper.HasOneToOneSignal(dependent)
                ? new Inverse(".WithOne()", true)
                : new Inverse(".WithMany()", false);

        private static Inverse InverseFor(string inverseTypeName, string inverseName) =>
            EfTypeHelper.IsCollectionType(inverseTypeName)
                ? new Inverse($".WithMany(x => x.{inverseName})", false)
                : new Inverse($".WithOne(x => x.{inverseName})", true);

        private static IEnumerable<(string name, bool isCollection)> FindInverseNavCandidates(
            EntityModel principal, string dependentEntityName, string? excludeNavName)
        {
            foreach (var p in principal.Properties)
            {
                if (excludeNavName != null && string.Equals(p.Name, excludeNavName, StringComparison.Ordinal))
                    continue;

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
            foreach (var fkName in fkProps)
            {
                var p = entity.Properties.FirstOrDefault(x => x.Name == fkName);
                if (p != null && p.IsNullable)
                    return false;
            }
            return true;
        }

        private static bool IsNavigationProperty(Property prop)
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
            if (attr == null)
                return null;

            return EfTypeHelper.NormalizeMemberName(attr.PositionalArgs.FirstOrDefault() as string ?? "");
        }

        private static string? GetDeleteBehavior(EntityModel entity, ForeignKeyPair pair)
        {
            var navProp = entity.Properties.FirstOrDefault(p => p.Name == pair.NavName);
            var fkProps = pair.FkPropertyNames
                .Select(name => entity.Properties.FirstOrDefault(p => p.Name == name));

            foreach (var candidate in new[] { navProp }.Concat(fkProps))
            {
                if (candidate == null)
                    continue;

                var attr = EfTypeHelper.TryGetDeleteBehaviorAttribute(candidate);
                if (attr == null)
                    continue;

                var arg = attr.PositionalArgs.FirstOrDefault() ?? attr.NamedArgs.GetValueOrDefault("behavior");
                if (!string.IsNullOrWhiteSpace(arg))
                    return FormatDeleteBehavior(arg);
            }

            return null;
        }

        private static string FormatDeleteBehavior(string arg)
        {
            return arg.Contains("DeleteBehavior.") ? arg : $"DeleteBehavior.{arg}";
        }
    }
}
