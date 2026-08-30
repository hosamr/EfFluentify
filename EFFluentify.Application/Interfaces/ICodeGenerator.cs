using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Interfaces;
using EFFluentify.Application.Models;

namespace EFFluentify.Application.Interfaces
{
    public interface ICodeGenerator
    {
        Dictionary<string, string> Generate(IEnumerable<EntityModel> entities, PipelineOptions options, IRuleRegistry rules);
    }

}
