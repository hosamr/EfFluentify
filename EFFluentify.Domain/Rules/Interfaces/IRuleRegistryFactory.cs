using EFFluentify.Domain.Models;

namespace EFFluentify.Domain.Rules.Interfaces
{
    public interface IRuleRegistryFactory
    {
        IRuleRegistry Create(IEnumerable<EntityModel>? allEntities = null);
    }
}
