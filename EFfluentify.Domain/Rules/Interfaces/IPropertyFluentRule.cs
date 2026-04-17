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
        bool CanApply(AttributeEntry attribute, Property property);
        IEnumerable<string> GetFluentLines(AttributeEntry attribute, Property property);
        public IEnumerable<string> GetAnnotationPropertyNames();

    }

}
