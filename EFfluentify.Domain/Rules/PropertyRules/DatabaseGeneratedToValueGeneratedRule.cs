using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class DatabaseGeneratedToValueGeneratedRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "DatabaseGenerated";

        public string? GetFluentCall(AttributeEntry attribute, Property property)
        {
            if (attribute.PositionalArgs is not { Count: > 0 } ctorArgs)
                return null;

            var arg = ctorArgs[0];

            return arg switch
            {
                "DatabaseGeneratedOption.Identity" => ".ValueGeneratedOnAdd()",
                "DatabaseGeneratedOption.Computed" => ".ValueGeneratedOnAddOrUpdate()",
                "DatabaseGeneratedOption.None" => ".ValueGeneratedNever()",
                _ => null
            };
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "DatabaseGenerated";
        }
    }
}
