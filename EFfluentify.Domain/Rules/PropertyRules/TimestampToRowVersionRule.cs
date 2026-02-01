using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.PropertyRules
{
    public sealed class TimestampToRowVersionRule : IPropertyFluentRule
    {
        public bool CanApply(AttributeModel attribute, PropertyModel property)
            => attribute.Name is "Timestamp";

        public string? GetFluentCall(AttributeModel attribute, PropertyModel property)
        {
            return ".IsRowVersion().IsConcurrencyToken().ValueGeneratedOnAddOrUpdate()";
        }
        public IEnumerable<string> GetAnnotationPropertyNames()
        {
            yield return "Timestamp";
        }
    }
}
