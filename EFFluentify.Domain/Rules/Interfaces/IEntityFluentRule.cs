using EFFluentify.Domain.Models;

namespace EFFluentify.Domain.Rules.Interfaces
{
    public interface IEntityFluentRule
    {
        bool CanApply(EntityModel entity);
        IEnumerable<string> GetFluentLines(EntityModel entity);
        public IEnumerable<string> GetAnnotationAttributeNames();

    }
}
