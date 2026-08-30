using EFFluentify.Domain.Helpers;
using EFFluentify.Domain.Models;

namespace EFFluentify.Domain.Rules.Helpers
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
            typeName = EfTypeHelper.NormalizeTypeName(typeName);
            return _byName.TryGetValue(typeName, out entity!);
            
        }
    }

}
