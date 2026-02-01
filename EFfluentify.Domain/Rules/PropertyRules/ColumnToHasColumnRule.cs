using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class ColumnToHasColumnRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeModel attribute, PropertyModel property)
            => attribute.Name is "Column";

        public string? GetFluentCall(AttributeModel attribute, PropertyModel property)
        {
            var parts = new List<string>();

            if (attribute.PositionalArgs is { Count: > 0 } ctorArgs &&
                ctorArgs[0] is string name && !string.IsNullOrWhiteSpace(name))
            {
                parts.Add($".HasColumnName({name})");
            }

            if (attribute.NamedArgs != null &&
                attribute.NamedArgs.TryGetValue("TypeName", out var typeNameObj) &&
                typeNameObj is string typeName &&
                !string.IsNullOrWhiteSpace(typeName))
            {
                parts.Add($".HasColumnType({typeName})");
            }

            if (parts.Count == 0)
                return null;

            return string.Concat(parts);
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Column";
        }

    }

}
