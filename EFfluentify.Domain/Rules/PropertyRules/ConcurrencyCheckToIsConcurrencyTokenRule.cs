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
        public bool CanApply(AttributeEntry attribute, Property property)
            => attribute.Name is "ConcurrencyCheck";

        public string? GetFluentCall(AttributeEntry attribute, Property property)
            => ".IsConcurrencyToken()";
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "ConcurrencyCheck";
        }

    }
}
