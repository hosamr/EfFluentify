using EFfluentify.Application.Models;

namespace EFfluentify.Application.Interfaces
{
    public interface IConvertAnnotationsService
    {
        Task<Dictionary<string, string>> Run(IEnumerable<string> inputs, PipelineOptions options);
    }
}
