using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class KeylessAttributeToHasNoKeyRule : IEntityFluentRule
    {
        public bool CanApply(EntityModel entity)
            => entity.Attributes.Any(a => a.Name is "Keyless" or "KeylessAttribute");

        public IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            yield return "builder.HasNoKey();";
        }

        public IEnumerable<string> GetAnnotationAttributeNames()
        {
            yield return "Keyless";
            yield return "KeylessAttribute";
        }
    }

}
