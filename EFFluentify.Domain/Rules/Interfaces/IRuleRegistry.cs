using EFFluentify.Domain.Models;

namespace EFFluentify.Domain.Rules.Interfaces
{
    public interface IRuleRegistry
    {
        IEnumerable<string> GetCallsForProperty(Property property);
        IEnumerable<string> GetCallsForEntity(EntityModel entity);
        IEnumerable<string> AllAnnotationAttributeNames();
    }
}
