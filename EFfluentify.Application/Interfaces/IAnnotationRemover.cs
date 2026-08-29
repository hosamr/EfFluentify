using EFfluentify.Application.Models;

namespace EFfluentify.Application.Interfaces
{
    public interface IAnnotationRemover
    {
        Task<IReadOnlyList<AnnotationRemovalChange>> PrepareRemovalAsync(IEnumerable<string> inputs);
    }
}
