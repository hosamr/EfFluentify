namespace EFfluentify.Application.Interfaces
{
    public interface IFileManager
    {
        Task<string> ReadFileAsync(string path);
        Task writeFilesToDiskAsync(Dictionary<string, string> results, string OutputDirectory);
        IEnumerable<string> ExpandFiles(IEnumerable<string> inputs);
        Task WriteFileAsync(string path, string content);
    }
}
