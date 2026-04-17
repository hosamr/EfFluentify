using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class ColumnToHasColumnRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "Column";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            if (attribute.PositionalArgs is { Count: > 0 } ctorArgs &&
                ctorArgs[0] is string name && !string.IsNullOrWhiteSpace(name))
            {
                yield return $".HasColumnName({name})";
            }

            if (attribute.NamedArgs != null &&
                attribute.NamedArgs.TryGetValue("TypeName", out var typeNameObj) &&
                typeNameObj is string typeName &&
                !string.IsNullOrWhiteSpace(typeName))
            {
                yield return $".HasColumnType({typeName})";
            }
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Column";
        }

    }

}
