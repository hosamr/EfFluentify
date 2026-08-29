using EFfluentify.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class TableAttributeToToTableRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Table" };

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var tableAttr = entity.Attributes.First(a => a.Name == "Table");

            var nameObj = tableAttr.PositionalArgs[0];
            tableAttr.NamedArgs.TryGetValue("Schema", out var schemaObj);

            var tableName = nameObj as string;
            var schema = schemaObj as string;

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
