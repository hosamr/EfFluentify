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
        public bool CanApply(AttributeModel attribute, PropertyModel property)
            => attribute.Name is "Nullable"
               && property.IsNullable
               && !LooksLikeForeignKey(property);

        public string? GetFluentCall(AttributeModel attribute, PropertyModel property)
            => ".IsRequired(false)";

        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Nullable";
        }

        private static bool LooksLikeForeignKey(PropertyModel p)
            => p.Name.EndsWith("Id", StringComparison.Ordinal);
    }


}
