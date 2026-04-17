using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public class PrecisionFluentRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "Precision" or "PrecisionAttribute";
        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            var precision = attribute.PositionalArgs.ElementAtOrDefault(0);
            var scale = attribute.PositionalArgs.ElementAtOrDefault(1);

            if (attribute.NamedArgs.TryGetValue("precision", out var namedPrecision))
                precision = namedPrecision;

            if (attribute.NamedArgs.TryGetValue("scale", out var namedScale))
                scale = namedScale;

            if (string.IsNullOrWhiteSpace(precision))
                yield break;

            if (!string.IsNullOrWhiteSpace(scale))
            {
                yield return $".HasPrecision({precision}, {scale})";
            }
            else
            {
                yield return $".HasPrecision({precision})";
            }
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Precision";
            yield return "PrecisionAttribute";
        }

    }
}
