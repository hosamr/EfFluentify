using EFfluentify.Domain.Models;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class CommentAttributeToHasCommentRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Comment" };

        protected override AttributeScope Scope => AttributeScope.Entity;

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var commentAttr = entity.Attributes.FirstOrDefault(IsSupportedAttribute);
            var comment = commentAttr?.PositionalArgs.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(comment))
                yield return $"builder.HasComment({comment});";
        }
    }
}
