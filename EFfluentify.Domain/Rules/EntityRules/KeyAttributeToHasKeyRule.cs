using EFfluentify.Domain.Helpers;
using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class KeyAttributeToHasKeyRule : IEntityFluentRule
    {
        public bool CanApply(EntityModel entity)
            => entity.Properties.Any(EfTypeHelper.HasKeyAttribute);

        public IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var keyProps = entity.Properties
                .Where(EfTypeHelper.HasKeyAttribute)
                .Select(p => p.Name)
                .ToList();

            if (keyProps.Count == 0)
                yield break;

            if (keyProps.Count == 1)
            {
                yield return $"builder.HasKey(e => e.{keyProps[0]});";
                yield break;
            }

            var anon = string.Join(", ", keyProps.Select(n => $"e.{n}"));
            yield return $"builder.HasKey(e => new {{ {anon} }});";
        }

        public IEnumerable<string> GetAnnotationAttributeNames()
        {
            yield return "Key";
            yield return "KeyAttribute";
        }
    }

}
