using EFFluentify.Application.Models;

namespace EFFluentify.Application.Interfaces
{
    public interface IConvertAnnotationsService
    {
        Task<Dictionary<string, string>> RunAsync(IEnumerable<string> inputs, PipelineOptions options);
    }
}
