using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Interfaces;

namespace EFFluentify.Domain.Rules.PropertyRules
{
    public sealed class RequiredToIsRequiredRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "Required";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            yield return ".IsRequired()";
        }

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Required";
        }
    }
}
