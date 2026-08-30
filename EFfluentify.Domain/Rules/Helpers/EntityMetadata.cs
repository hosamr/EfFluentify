using EFfluentify.Domain.Helpers;
using EFfluentify.Domain.Models;

namespace EFfluentify.Domain.Rules.Helpers
{
    public sealed class EntityMetadata
    {
        private readonly IReadOnlySet<string> _entityNames;
        private readonly HashSet<string> _keyProperties;
        private readonly HashSet<string> _ignoredProperties;
        private readonly HashSet<string> _foreignKeyProperties;

        public EntityMetadata(EntityModel entity, IReadOnlyList<EntityModel> allEntities)
            : this(entity, EfTypeHelper.BuildEntityNameSet(allEntities))
        {
        }

        public EntityMetadata(EntityModel entity, IReadOnlySet<string> entityNames)
        {
            _entityNames = entityNames;
            _keyProperties = GetKeyProperties(entity);
            _ignoredProperties = GetIgnoredProperties(entity);
            _foreignKeyProperties = GetForeignKeyProperties(entity);
        }

        public bool IsNavigationProperty(Property prop) => EfTypeHelper.IsNavigationProperty(prop, _entityNames);
        public bool IsIgnored(string propName) => _ignoredProperties.Contains(propName);
        public bool IsKey(string propName) => _keyProperties.Contains(propName);
        public bool IsForeignKey(string propName) => _foreignKeyProperties.Contains(propName);

        private static HashSet<string> GetKeyProperties(EntityModel entity)
        {
            return entity.Properties
                .Where(EfTypeHelper.HasKeyAttribute)
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> GetIgnoredProperties(EntityModel entity)
        {
            return entity.Properties
                .Where(EfTypeHelper.HasNotMappedAttribute)
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private HashSet<string> GetForeignKeyProperties(EntityModel entity)
        {
            var fkPropNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in entity.Properties)
            {
                var fkAttr = EfTypeHelper.TryGetForeignKeyAttribute(prop);
                if (fkAttr == null)
                    continue;

                var arg0 = fkAttr.PositionalArgs.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(arg0))
                    continue;

                if (EfTypeHelper.IsNavigationProperty(prop, _entityNames) || EfTypeHelper.IsCollectionType(prop.TypeName))
                {
                    foreach (var fk in EfTypeHelper.SplitFkNames(arg0))
                        fkPropNames.Add(fk);
                }
                else
                {
                    fkPropNames.Add(prop.Name);
                }
            }
            return fkPropNames;
        }
    }
}
