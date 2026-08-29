using EFfluentify.Domain.Models;
using System.Collections.Generic;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class KeylessAttributeToHasNoKeyRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Keyless" };

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            yield return "builder.HasNoKey();";
        }
    }
}
