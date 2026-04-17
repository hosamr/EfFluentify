using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;


namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class MaxLengthToHasMaxLengthRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "MaxLength" or "StringLength";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            var hasPos = attribute.PositionalArgs.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(hasPos))
            {
                yield return $".HasMaxLength({hasPos})";
            }

            else if (attribute.NamedArgs.TryGetValue("MaximumLength", out var max))
            {
                yield return $".HasMaxLength({max})";
            }
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "MaxLength";
            yield return "StringLength";
        }
    }
}
