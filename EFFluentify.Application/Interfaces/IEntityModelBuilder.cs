using EFFluentify.Domain.Models;

namespace EFFluentify.Application.Interfaces
{
    public interface IEntityModelBuilder
    {
        Task<List<EntityModel>> BuildFromInputsAsync(IEnumerable<string> inputs);
    }
}
