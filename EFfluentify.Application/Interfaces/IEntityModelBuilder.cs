using EFfluentify.Domain.Models;

namespace EFfluentify.Application.Interfaces
{
    public interface IEntityModelBuilder
    {
        Task<List<EntityModel>> BuildFromInputs(IEnumerable<string> inputs);
    }
}
