using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class NullablePropertyFluentRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "Nullable"
               && property.IsNullable
               && !LooksLikeForeignKey(property);

        public IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property)
        {
            yield return ".IsRequired(false)";
        }

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Nullable";
        }

        private static bool LooksLikeForeignKey(Property p)
            => p.Name.EndsWith("Id", StringComparison.Ordinal);
    }


}
