using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class UnicodeFluentRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "Unicode" or "UnicodeAttribute";

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {

            if (attribute.PositionalArgs.Count > 0)
            {
                bool isUnicode;

                if (Boolean.TryParse(attribute.PositionalArgs[0], out isUnicode))
                {
                    yield return $".IsUnicode({isUnicode.ToString().ToLowerInvariant()})";
                    yield break;
                }
            }
            yield return ".IsUnicode()";
        }

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Unicode";
            yield return "UnicodeAttribute";
        }
    }

}
