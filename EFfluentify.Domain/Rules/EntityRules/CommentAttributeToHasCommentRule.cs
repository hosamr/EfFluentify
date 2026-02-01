using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class CommentAttributeToHasCommentRule : IEntityFluentRule
    {
        public bool CanApply(EntityModel entity)
            => entity.Attributes.Any(a => a.Name is "Comment" or "CommentAttribute");

        public IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var commentAttr = entity.Attributes.First(a => a.Name is "Comment" or "CommentAttribute");

            object? commentObj = commentAttr.PositionalArgs.Count > 0 ? commentAttr.PositionalArgs[0] : null;
            var comment = commentObj as string;

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
