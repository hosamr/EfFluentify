using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class TableAttributeToToTableRule : IEntityFluentRule
    {
        public bool CanApply(EntityModel entity)
            => entity.Attributes.Any(a => a.Name == "Table");

        public IEnumerable<string> GetFluentLines(EntityModel entity)
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
        public IEnumerable<string> GetAnnotationAttributeNames()
        {
            yield return "Table";
            yield return "TableAttribute";
        }

    }
}
