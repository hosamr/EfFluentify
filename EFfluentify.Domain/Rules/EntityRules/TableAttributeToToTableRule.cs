using EFfluentify.Domain.Models;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class TableAttributeToToTableRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Table" };

        protected override AttributeScope Scope => AttributeScope.Entity;

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var tableAttr = entity.Attributes.FirstOrDefault(IsSupportedAttribute);
            if (tableAttr == null)
                yield break;

            var tableName = tableAttr.PositionalArgs.FirstOrDefault();
            tableAttr.NamedArgs.TryGetValue("Schema", out var schema);

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                if (!string.IsNullOrWhiteSpace(schema))
                    yield return $"builder.ToTable({tableName}, {schema});";
                else
                    yield return $"builder.ToTable({tableName});";
            }
            else
            {
                yield return $"builder.ToTable(\"{entity.Name}\");";
            }
        }
    }
}
