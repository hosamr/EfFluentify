using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class DatabaseGeneratedToValueGeneratedRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "DatabaseGenerated";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            if (attribute.PositionalArgs is not { Count: > 0 } ctorArgs)
                yield break;

            var arg = ctorArgs[0];

            var call = arg switch
            {
                "DatabaseGeneratedOption.Identity" => ".ValueGeneratedOnAdd()",
                "DatabaseGeneratedOption.Computed" => ".ValueGeneratedOnAddOrUpdate()",
                "DatabaseGeneratedOption.None" => ".ValueGeneratedNever()",
                _ => null
            };
            if (call != null) yield return call;
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "DatabaseGenerated";
        }
    }
}
