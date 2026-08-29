using EFfluentify.Domain.Models;

namespace EFfluentify.Domain.Rules.Interfaces
{
    public interface IEntityFluentRule
    {
        bool CanApply(EntityModel entity);
        IEnumerable<string> GetFluentLines(EntityModel entity);
        public IEnumerable<string> GetAnnotationAttributeNames();

    }
}
