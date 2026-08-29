using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class CommentToHasCommentRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "Comment" or "CommentAttribute";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            var comment = attribute.PositionalArgs.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(comment))
            {
                yield return $".HasComment({comment})";
            }
        }

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Comment";
            yield return "CommentAttribute";
        }
    }
}
