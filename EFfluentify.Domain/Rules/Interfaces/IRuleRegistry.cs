using EFfluentify.Domain.Models;

namespace EFfluentify.Domain.Rules.Interfaces
{
    public interface IRuleRegistry
    {
        IEnumerable<string> GetCallsForProperty(Property property);
        IEnumerable<string> GetCallsForEntity(EntityModel entity);
        IEnumerable<string> AllAnnotationAttributeNames();
    }
}
