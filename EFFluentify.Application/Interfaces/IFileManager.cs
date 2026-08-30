namespace EFFluentify.Application.Interfaces
{
    public interface IFileManager
    {
        Task<string> ReadFileAsync(string path);
        Task WriteFilesToDiskAsync(Dictionary<string, string> results, string outputDirectory);
        IEnumerable<string> ExpandFiles(IEnumerable<string> inputs);
        Task WriteFileAsync(string path, string content);
    }
}
