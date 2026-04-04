using EFfluentify.Application.Interfaces;
namespace EFfluentify.Infrastructure.IO
{
    public class FileManager : IFileManager
    {
        public async Task<string> ReadFileAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            return  await File.ReadAllTextAsync(path);
        }

        public async Task writeFilesToDiskAsync(Dictionary<string, string> results, string OutputDirectory)
        {
            EnsureDirectory(OutputDirectory);

            var tasks = results.Select(async kv =>
            {
                var filePath = Path.Combine(OutputDirectory, kv.Key);
                await WriteFileAsync(filePath, kv.Value);
                Console.WriteLine($"Wrote: {filePath}");
            });

            await Task.WhenAll(tasks);
        }
        
        public IEnumerable<string> ExpandFiles(IEnumerable<string> inputs)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var path in inputs)
            {
                if (FileExists(path)) set.Add(Path.GetFullPath(path));
                else if (Directory.Exists(path))
                {
                    foreach (var f in Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories))
                        set.Add(Path.GetFullPath(f));
                }
            }
            return set;
        }
        
        private void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public async Task WriteFileAsync(string path, string content)
        {
            await File.WriteAllTextAsync(path, content);
        }
        private bool FileExists(string path) => File.Exists(path);


    }
}
