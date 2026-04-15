using EFfluentify.Domain.Models;

namespace EFfluentify.Domain.Rules.Interfaces
{
    public interface IRuleRegistryFactory
    {
        IRuleRegistry Create(IEnumerable<EntityModel>? allEntities = null);
    }
}
