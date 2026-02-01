using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class RequiredToIsRequiredRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeModel attribute, PropertyModel property)
            => attribute.Name is "Required";

        public string? GetFluentCall(AttributeModel attribute, PropertyModel property)
            => ".IsRequired()";
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Required";
        }
    }

}
