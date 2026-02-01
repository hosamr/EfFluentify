using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public class PrecisionFluentRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeModel attribute, PropertyModel property)
            => attribute.Name is "Precision" or "PrecisionAttribute";
        public string? GetFluentCall(AttributeModel attribute, PropertyModel property)
        {
            var precision = attribute.PositionalArgs.ElementAtOrDefault(0);
            var scale = attribute.PositionalArgs.ElementAtOrDefault(1);

            if (attribute.NamedArgs.TryGetValue("precision", out var namedPrecision))
                precision = namedPrecision;

            if (attribute.NamedArgs.TryGetValue("scale", out var namedScale))
                scale = namedScale;

            if (string.IsNullOrWhiteSpace(precision))
                return null;

            if (!string.IsNullOrWhiteSpace(scale))
                return $".HasPrecision({precision}, {scale})";

            return $".HasPrecision({precision})";
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Precision";
            yield return "PrecisionAttribute";
        }

    }
}
