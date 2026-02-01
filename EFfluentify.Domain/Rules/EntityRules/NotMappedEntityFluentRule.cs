using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class NotMappedEntityFluentRule : IEntityFluentRule
    {
        public bool CanApply(EntityModel entity)
            => entity.Properties.Any(p =>
                p.Attributes.Any(a =>
                    a.Name is "NotMapped" or "NotMappedAttribute"));

        public IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            foreach (var property in entity.Properties)
            {
                if (property.Attributes.Any(a =>
                    a.Name is "NotMapped" or "NotMappedAttribute"))
                {
                    yield return $"builder.Ignore(e => e.{property.Name});";
                }
            }
        }

        public IEnumerable<string> GetAnnotationAttributeNames()
        {
            yield return "NotMapped";
            yield return "NotMappedAttribute";
        }
    }

}
