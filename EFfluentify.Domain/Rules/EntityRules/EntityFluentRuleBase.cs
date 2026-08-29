using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public abstract class EntityFluentRuleBase : IEntityFluentRule
    {
        protected abstract IEnumerable<string> SupportedAttributeNames { get; }

        protected abstract AttributeScope Scope { get; }

        public virtual bool CanApply(EntityModel entity)
        {
            return Scope == AttributeScope.Entity
                ? entity.Attributes.Any(IsSupportedAttribute)
                : entity.Properties.Any(p => p.Attributes.Any(IsSupportedAttribute));
        }

        public abstract IEnumerable<string> GetFluentLines(EntityModel entity);

        public virtual IEnumerable<string> GetAnnotationAttributeNames()
        {
            foreach (var name in SupportedAttributeNames)
            {
                yield return name;
                if (!name.EndsWith("Attribute", StringComparison.Ordinal))
                {
                    yield return name + "Attribute";
                }
            }
        }

        protected bool IsSupportedAttribute(AttributeEntry attribute)
        {
            return SupportedAttributeNames.Any(name =>
                attribute.Name.Equals(name, StringComparison.Ordinal) ||
                attribute.Name.Equals(name + "Attribute", StringComparison.Ordinal));
        }
    }
}
