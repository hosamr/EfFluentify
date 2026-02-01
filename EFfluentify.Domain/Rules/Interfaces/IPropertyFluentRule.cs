using EFfluentify.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.Interfaces
{
    public interface IPropertyFluentRule
    {
        bool CanApply(AttributeModel attribute, PropertyModel property);
        string? GetFluentCall(AttributeModel attribute, PropertyModel property);
        public IEnumerable<string> GetAnnotationPropertyNames();

    }

}
