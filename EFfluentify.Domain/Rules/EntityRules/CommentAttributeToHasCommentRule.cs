using EFfluentify.Domain.Helpers;
using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class CommentAttributeToHasCommentRule : IEntityFluentRule
    {
        public bool CanApply(EntityModel entity)
            => entity.Attributes.Any(EfTypeHelper.IsCommentAttribute);

        public IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var commentAttr = entity.Attributes.First(EfTypeHelper.IsCommentAttribute);

            var comment = commentAttr.PositionalArgs.Count > 0
                ? commentAttr.PositionalArgs[0] as string
                : null;

            if (!string.IsNullOrWhiteSpace(comment))
                yield return $"builder.HasComment({comment});";
        }

        public IEnumerable<string> GetAnnotationAttributeNames()
        {
            yield return "Comment";
            yield return "CommentAttribute";
        }
    }
}
