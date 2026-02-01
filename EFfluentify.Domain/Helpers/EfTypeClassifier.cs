using EFfluentify.Domain.Models;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EFfluentify.Application.Helpers
{
    public static class EfTypeClassifier
    {
        public static bool IsArrayType(string typeName) => typeName.Trim().EndsWith("[]", StringComparison.Ordinal);
        public static bool IsCollectionType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return false;

            var t = typeName.Trim();

            if (IsArrayType(t))
                return true;

            var name = t.Split('.').Last();
            var baseName = name.Split('<')[0];

            return CollectionTypeNames.Contains(baseName, StringComparer.Ordinal);
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

            t = t.TrimEnd('?');

            return ScalarTypeNames.Contains(t, StringComparer.Ordinal);
        }
        public static bool IsNavigationProperty(PropertyModel p, IEnumerable<EntityModel> entities)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            var entityNames = new HashSet<string>(entities.Select(e => e.Name), StringComparer.Ordinal);

            var typeName = GetTypeName(p);
            if (string.IsNullOrWhiteSpace(typeName))
                return false;
            typeName = Normalize(typeName);

            if (entityNames.Contains(typeName))
                return true;

            if (TryGetGenericArgument(typeName, out var genericArg))
            {
                var outer = GetOuterGenericType(typeName);
                if (outer != null && CollectionTypeNames.Contains(outer, StringComparer.Ordinal))
                {
                    genericArg = Normalize(genericArg);
                    if (entityNames.Contains(genericArg))
                        return true;
                }
            }

            if (typeName.EndsWith("[]", StringComparison.Ordinal))
            {
                var elementType = typeName[..^2];
                elementType = Normalize(elementType);
                if (entityNames.Contains(elementType))
                    return true;
            }

            return false;
        }

        private static string Normalize(string typeName)
        {
            typeName = typeName.Trim();
            if (typeName.EndsWith("?", StringComparison.Ordinal))
                typeName = typeName[..^1];

            var lastDot = typeName.LastIndexOf('.');
            if (lastDot >= 0)
                typeName = typeName[(lastDot + 1)..];

            typeName = Regex.Replace(typeName, @"\s+", "");

            return typeName;
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

            var outer = typeName.Substring(0, lt);
            outer = Normalize(outer);
            return outer;
        }

        private static string GetTypeName(PropertyModel p)
        {
            return p.TypeName;
        }

        private static readonly string[] CollectionTypeNames = {
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
        private static readonly string[] ScalarTypeNames = {
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