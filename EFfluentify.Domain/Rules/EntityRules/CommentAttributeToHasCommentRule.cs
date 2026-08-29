using EFfluentify.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class CommentAttributeToHasCommentRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Comment" };

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var commentAttr = entity.Attributes.First(IsSupportedAttribute);

            var comment = commentAttr.PositionalArgs.Count > 0
                ? commentAttr.PositionalArgs[0] as string
                : null;

            if (!string.IsNullOrWhiteSpace(comment))
                yield return $"builder.HasComment({comment});";
        }
    }
}
