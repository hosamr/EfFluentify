using EFfluentify.Application.Helpers;
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
            typeName = EfTypeClassifier.NormalizeTypeName(typeName);
            return _byName.TryGetValue(typeName, out entity!);
            
        }
    }

}
