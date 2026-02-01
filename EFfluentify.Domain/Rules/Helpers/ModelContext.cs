using EFfluentify.Domain.Models;

namespace EFfluentify.Domain.Rules.Helpers
{
    public sealed class ModelContext
    {
        private readonly Dictionary<string, EntityModel> _byName;

        public ModelContext(IEnumerable<EntityModel> entities)
        {
            _byName = (entities ?? Enumerable.Empty<EntityModel>())
                        .Where(e => !string.IsNullOrWhiteSpace(e.Name))
                        .GroupBy(e => e.Name)
                        .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        }

        public bool TryGetEntity(string typeName, out EntityModel entity)
        {
            typeName = NormalizeTypeName(typeName);
            return _byName.TryGetValue(typeName, out entity!);
        }

        private static string NormalizeTypeName(string typeName)
        {
            var t = typeName.Trim();

            // strip nullable markers like "Customer?" if you keep them in TypeName
            t = t.TrimEnd('?');

            // strip namespace if TypeName is "MyApp.Models.Customer"
            var lastDot = t.LastIndexOf('.');
            if (lastDot >= 0)
                t = t[(lastDot + 1)..];

            return t;
        }
    }

}
