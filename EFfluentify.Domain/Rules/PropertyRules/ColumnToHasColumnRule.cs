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
            var name = attribute.PositionalArgs.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(name))
            {
                yield return $".HasColumnName({name})";
            }

            if (attribute.NamedArgs.TryGetValue("TypeName", out var typeName) &&
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
