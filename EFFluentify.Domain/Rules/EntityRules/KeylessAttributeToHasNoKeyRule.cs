using EFFluentify.Domain.Models;

namespace EFFluentify.Domain.Rules.EntityRules
{
    public sealed class KeylessAttributeToHasNoKeyRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Keyless" };

        protected override AttributeScope Scope => AttributeScope.Entity;

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            yield return "builder.HasNoKey();";
        }
    }
}
