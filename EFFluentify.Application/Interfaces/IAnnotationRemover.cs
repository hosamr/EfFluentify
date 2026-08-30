using EFFluentify.Application.Models;

namespace EFFluentify.Application.Interfaces
{
    public interface IAnnotationRemover
    {
        Task<IReadOnlyList<AnnotationRemovalChange>> PrepareRemovalAsync(IEnumerable<string> inputs);
    }
}
