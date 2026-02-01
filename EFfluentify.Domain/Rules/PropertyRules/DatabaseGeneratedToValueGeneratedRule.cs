using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class DatabaseGeneratedToValueGeneratedRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeModel attribute, PropertyModel property)
            => attribute.Name is "DatabaseGenerated";

        public string? GetFluentCall(AttributeModel attribute, PropertyModel property)
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
