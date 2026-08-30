using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Interfaces;

namespace EFFluentify.Domain.Rules.PropertyRules
{
    public sealed class ConcurrencyCheckToIsConcurrencyTokenRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "ConcurrencyCheck";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            yield return ".IsConcurrencyToken()";
        }

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "ConcurrencyCheck";
        }
    }
}
