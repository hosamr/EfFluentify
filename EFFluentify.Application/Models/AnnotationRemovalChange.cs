namespace EFFluentify.Application.Models
{
    public sealed class AnnotationRemovalChange
    {
        public string FilePath { get; }
        public string OriginalContent { get; }
        public string UpdatedContent { get; }

        public AnnotationRemovalChange(string filePath, string originalContent, string updatedContent)
        {
            FilePath = filePath;
            OriginalContent = originalContent;
            UpdatedContent = updatedContent;
        }
    }
}
