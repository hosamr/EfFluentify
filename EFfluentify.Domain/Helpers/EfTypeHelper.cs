using EFfluentify.Domain.Models;
using System.Text.RegularExpressions;

namespace EFfluentify.Domain.Helpers
{
    public static class EfTypeHelper
    {
        public static bool IsCollectionType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return false;

            var t = typeName.Trim();

            if (IsArrayType(t))
                return true;

            var baseName = t.Split('.').Last().Split('<')[0];
            return _collectionTypeNames.Contains(baseName);
        }

        public static string? TryGetCollectionElementType(string typeName)
        {
            var t = typeName.Trim();

            int lt = t.IndexOf('<');
            int gt = t.LastIndexOf('>');
            if (lt < 0 || gt <= lt)
                return null;

            return t.Substring(lt + 1, gt - lt - 1).Trim().TrimEnd('?');
        }

        public static bool IsScalarType(string typeName)
        {
            var t = typeName.Trim();

            if (IsArrayType(t))
                return true;

            return _scalarTypeNames.Contains(t.TrimEnd('?'));
        }

        public static HashSet<string> BuildEntityNameSet(IEnumerable<EntityModel> entities)
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            return new HashSet<string>(entities.Select(e => e.Name), StringComparer.Ordinal);
        }

        public static bool IsNavigationProperty(Property p, IEnumerable<EntityModel> entities)
            => IsNavigationProperty(p, BuildEntityNameSet(entities));

        public static bool IsNavigationProperty(Property p, IReadOnlySet<string> entityNames)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (entityNames == null) throw new ArgumentNullException(nameof(entityNames));

            var typeName = p.TypeName;

            if (string.IsNullOrWhiteSpace(typeName))
                return false;

            typeName = NormalizeTypeName(typeName);

            if (entityNames.Contains(typeName))
                return true;

            if (TryGetGenericArgument(typeName, out var genericArg))
            {
                var outer = GetOuterGenericType(typeName);
                if (outer != null && _collectionTypeNames.Contains(outer))
                {
                    if (entityNames.Contains(NormalizeTypeName(genericArg)))
                        return true;
                }
            }

            if (IsArrayType(typeName))
            {
                var elementType = NormalizeTypeName(typeName[..^2]);
                if (entityNames.Contains(elementType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Strips nullable markers, namespace prefixes, and internal whitespace
        /// from a type name. E.g. "MyApp.Models.Customer?" → "Customer".
        /// </summary>
        public static string NormalizeTypeName(string typeName)
        {
            var t = typeName.Trim();
            t = t.TrimEnd('?');

            var lastDot = t.LastIndexOf('.');
            if (lastDot >= 0)
                t = t[(lastDot + 1)..];

            return Regex.Replace(t, @"\s+", "");
        }

        /// <summary>
        /// Strips quotes and nameof() wrappers from a member name literal.
        /// Returns null when the result is empty.
        /// </summary>
        public static string? NormalizeMemberName(string raw)
        {
            var s = raw.Trim();

            if (s.StartsWith("nameof(", StringComparison.Ordinal) && s.EndsWith(")", StringComparison.Ordinal))
                s = s.Substring("nameof(".Length, s.Length - "nameof(".Length - 1).Trim();

            s = s.Trim('"');
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        /// <summary>
        /// Splits a comma-separated foreign-key name string (after normalising each part).
        /// </summary>
        public static IEnumerable<string> SplitFkNames(string raw)
        {
            var normalized = NormalizeMemberName(raw);
            if (string.IsNullOrWhiteSpace(normalized))
                yield break;

            foreach (var part in normalized.Split(','))
            {
                var p = part.Trim();
                if (!string.IsNullOrWhiteSpace(p))
                    yield return p;
            }
        }

        public static bool IsKeyAttribute(AttributeEntry a)
            => a.Name is "Key" or "KeyAttribute";

        public static bool IsForeignKeyAttribute(AttributeEntry a)
            => a.Name is "ForeignKey" or "ForeignKeyAttribute";

        public static bool IsNotMappedAttribute(AttributeEntry a)
            => a.Name is "NotMapped" or "NotMappedAttribute";

        public static bool IsInversePropertyAttribute(AttributeEntry a)
            => a.Name is "InverseProperty" or "InversePropertyAttribute";

        public static bool IsDeleteBehaviorAttribute(AttributeEntry a)
            => a.Name is "DeleteBehavior" or "DeleteBehaviorAttribute";

        public static bool HasKeyAttribute(Property p)
            => p.Attributes.Any(IsKeyAttribute);

        public static bool HasForeignKeyAttribute(Property p)
            => p.Attributes.Any(IsForeignKeyAttribute);

        public static bool HasNotMappedAttribute(Property p)
            => p.Attributes.Any(IsNotMappedAttribute);

        public static AttributeEntry? TryGetForeignKeyAttribute(Property p)
            => p.Attributes.FirstOrDefault(IsForeignKeyAttribute);

        public static AttributeEntry? TryGetInversePropertyAttribute(Property p)
            => p.Attributes.FirstOrDefault(IsInversePropertyAttribute);

        public static AttributeEntry? TryGetDeleteBehaviorAttribute(Property p)
            => p.Attributes.FirstOrDefault(IsDeleteBehaviorAttribute);

        /// <summary>
        /// Returns true when the dependent entity shows a shared-primary-key
        /// pattern: a property that carries both [Key] and [ForeignKey].
        /// This signals a one-to-one relationship.
        /// </summary>
        public static bool HasOneToOneSignal(EntityModel entity)
        {
            foreach (var p in entity.Properties)
            {
                if (HasKeyAttribute(p) && HasForeignKeyAttribute(p))
                    return true;
            }
            return false;
        }

        private static bool TryGetGenericArgument(string typeName, out string arg)
        {
            arg = "";

            var lt = typeName.IndexOf('<');
            var gt = typeName.LastIndexOf('>');
            if (lt < 0 || gt < 0 || gt <= lt)
                return false;

            var inside = typeName.Substring(lt + 1, gt - lt - 1);

            var comma = inside.IndexOf(',');
            if (comma >= 0)
                inside = inside.Substring(0, comma);

            arg = inside;
            return true;
        }

        private static string? GetOuterGenericType(string typeName)
        {
            var lt = typeName.IndexOf('<');
            if (lt <= 0)
                return null;

            return NormalizeTypeName(typeName.Substring(0, lt));
        }

        private static bool IsArrayType(string typeName)
            => typeName.Trim().EndsWith("[]", StringComparison.Ordinal);


        private static readonly HashSet<string> _collectionTypeNames = new(StringComparer.Ordinal)
        {
            "ICollection",
            "IEnumerable",
            "IList",
            "List",
            "HashSet",
            "Collection",
            "ObservableCollection",
            "IReadOnlyCollection",
            "IReadOnlyList"
        };

        private static readonly HashSet<string> _scalarTypeNames = new(StringComparer.Ordinal)
        {
            "string",
            "bool",
            "byte",
            "sbyte",
            "short",
            "ushort",
            "int",
            "uint",
            "long",
            "ulong",
            "float",
            "double",
            "decimal",
            "Guid",
            "DateTime",
            "DateTimeOffset"
        };
    }
}
