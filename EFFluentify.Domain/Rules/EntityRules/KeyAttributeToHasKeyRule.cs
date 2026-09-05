using EFFluentify.Domain.Models;

namespace EFFluentify.Domain.Rules.EntityRules
{
    public sealed class KeyAttributeToHasKeyRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Key" };

        protected override AttributeScope Scope => AttributeScope.Property;

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var keyProps = entity.Properties
                .Where(p => p.Attributes.Any(IsSupportedAttribute))
                .OrderBy(GetColumnOrder)
                .Select(p => p.Name)
                .ToList();

            if (keyProps.Count > 0)
            {
                yield return keyProps.Count == 1
                    ? $"builder.HasKey(e => e.{keyProps[0]});"
                    : $"builder.HasKey(e => new {{ {string.Join(", ", keyProps.Select(n => $"e.{n}"))} }});";
            }
        }

        private static int GetColumnOrder(Property p)
        {
            var column = p.Attributes.FirstOrDefault(a => a.Name is "Column" or "ColumnAttribute");
            if (column != null
                && column.NamedArgs.TryGetValue("Order", out var raw)
                && int.TryParse(raw, out var order))
            {
                return order;
            }

            return int.MaxValue;
        }
    }
}
