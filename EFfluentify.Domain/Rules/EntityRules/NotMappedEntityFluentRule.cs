using EFfluentify.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class NotMappedEntityFluentRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "NotMapped" };

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            foreach (var property in entity.Properties)
            {
                if (property.Attributes.Any(IsSupportedAttribute))
                    yield return $"builder.Ignore(e => e.{property.Name});";
            }
        }
    }
}
