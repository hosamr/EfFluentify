using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Interfaces;

namespace EFFluentify.Domain.Rules.PropertyRules
{
    public sealed class UnicodeFluentRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "Unicode" or "UnicodeAttribute";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            var arg = attribute.PositionalArgs.FirstOrDefault();
            if (bool.TryParse(arg, out var isUnicode))
            {
                yield return $".IsUnicode({isUnicode.ToString().ToLowerInvariant()})";
                yield break;
            }

            yield return ".IsUnicode()";
        }

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Unicode";
            yield return "UnicodeAttribute";
        }
    }
}
