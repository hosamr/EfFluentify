using EFfluentify.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.Interfaces
{
    public interface IEntityFluentRule
    {
        bool CanApply(EntityModel entity);
        IEnumerable<string> GetFluentLines(EntityModel entity);
        public IEnumerable<string> GetAnnotationAttributeNames();

    }
}
