using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Interfaces;

namespace EFFluentify.Domain.Rules.PropertyRules
{
    public sealed class MaxLengthToHasMaxLengthRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "MaxLength" or "StringLength";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            var length = attribute.PositionalArgs.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(length))
            {
                yield return $".HasMaxLength({length})";
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
