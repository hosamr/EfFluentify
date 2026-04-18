using EFfluentify.Domain.Helpers;
using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class NotMappedEntityFluentRule : IEntityFluentRule
    {
        public bool CanApply(EntityModel entity)
            => entity.Properties.Any(EfTypeHelper.HasNotMappedAttribute);

        public IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            foreach (var property in entity.Properties)
            {
                if (EfTypeHelper.HasNotMappedAttribute(property))
                    yield return $"builder.Ignore(e => e.{property.Name});";
            }
        }

        public IEnumerable<string> GetAnnotationAttributeNames()
        {
            yield return "NotMapped";
            yield return "NotMappedAttribute";
        }
    }

}
