using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class ConcurrencyCheckToIsConcurrencyTokenRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeModel attribute, PropertyModel property)
            => attribute.Name is "ConcurrencyCheck";

        public string? GetFluentCall(AttributeModel attribute, PropertyModel property)
            => ".IsConcurrencyToken()";
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "ConcurrencyCheck";
        }

    }
}
