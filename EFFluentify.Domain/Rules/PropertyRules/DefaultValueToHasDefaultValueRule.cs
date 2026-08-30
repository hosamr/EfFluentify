using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Interfaces;

namespace EFFluentify.Domain.Rules.PropertyRules
{
    public sealed class DefaultValueToHasDefaultValueRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "DefaultValue" or "DefaultValueAttribute";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            var value = attribute.PositionalArgs.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(value))
            {
                yield return $".HasDefaultValue({value})";
            }
        }

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "DefaultValue";
            yield return "DefaultValueAttribute";
        }
    }
}
