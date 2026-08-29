using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;
using EFfluentify.Application.Models;

namespace EFfluentify.Application.Interfaces
{
    public interface ICodeGenerator
    {
        Dictionary<string, string> Generate(IEnumerable<EntityModel> entities, PipelineOptions options, IRuleRegistry rules);
    }

}
