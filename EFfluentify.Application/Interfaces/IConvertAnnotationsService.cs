using EFfluentify.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EFfluentify.Application.Interfaces
{
    public interface IConvertAnnotationsService
    {
        Task<Dictionary<string, string>> Run(IEnumerable<string> inputs, PipelineOptions options);
    }
}
