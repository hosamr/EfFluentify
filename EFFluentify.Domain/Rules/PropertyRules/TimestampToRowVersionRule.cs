using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Interfaces;

namespace EFFluentify.Domain.Rules.PropertyRules
{
    public sealed class TimestampToRowVersionRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "Timestamp";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            yield return ".IsRowVersion()";
            yield return ".IsConcurrencyToken()";
            yield return ".ValueGeneratedOnAddOrUpdate()";
        }

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Timestamp";
        }
    }
}
