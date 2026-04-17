using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class MinLengthToHasMinLengthRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "MinLength" or "StringLength";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            var hasPos = attribute.PositionalArgs.FirstOrDefault();
            if (attribute.Name == "MinLength" && !string.IsNullOrWhiteSpace(hasPos))
            {
                yield return $".HasMinLength({hasPos})";
            }

            else if (attribute.NamedArgs.TryGetValue("MinimumLength", out var min))
            {
                yield return $".HasMinLength({min})";
            }
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "MinLength";
            yield return "StringLength";
        }
    }
}