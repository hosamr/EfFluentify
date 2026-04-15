using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;


namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class MaxLengthToHasMaxLengthRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "MaxLength" or "StringLength";

        public string? GetFluentCall(AttributeEntry attribute, Property property)
        {
            var hasPos = attribute.PositionalArgs.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(hasPos))
            {
                return $".HasMaxLength({hasPos})";
            }

            if (attribute.NamedArgs.TryGetValue("MaximumLength", out var max))
            {
                return $".HasMaxLength({max})";
            }

            return null;
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "MaxLength";
            yield return "StringLength";
        }
    }
}
