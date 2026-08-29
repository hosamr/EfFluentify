using EFfluentify.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class KeyAttributeToHasKeyRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Key" };

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var keyProps = entity.Properties
                .Where(p => p.Attributes.Any(IsSupportedAttribute))
                .Select(p => p.Name)
                .ToList();

            if (keyProps.Count > 0)
            {
                yield return keyProps.Count == 1
                    ? $"builder.HasKey(e => e.{keyProps[0]});"
                    : $"builder.HasKey(e => new {{ {string.Join(", ", keyProps.Select(n => $"e.{n}"))} }});";
            }
        }
    }
}
