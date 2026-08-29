
namespace EFfluentify.Application.Models
{
    public sealed class AnnotationRemovalChange
    {
        public string FilePath { get; }
        public string OriginalContent { get; }
        public string UpdatedContent { get; }

        public bool HasChanges =>
            !string.Equals(OriginalContent, UpdatedContent, StringComparison.Ordinal);

        public AnnotationRemovalChange(string filePath, string originalContent, string updatedContent)
        {
            FilePath = filePath;
            OriginalContent = originalContent;
            UpdatedContent = updatedContent;
        }
    }
}
