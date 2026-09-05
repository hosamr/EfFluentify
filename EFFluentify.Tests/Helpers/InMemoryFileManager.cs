using EFFluentify.Application.Interfaces;

namespace EFFluentify.Tests.Helpers
{
    internal sealed class InMemoryFileManager : IFileManager
    {
        private readonly Dictionary<string, string> _files;

        public List<(string Path, string Content)> Writes { get; } = new();

        public InMemoryFileManager(Dictionary<string, string> files)
        {
            _files = new Dictionary<string, string>(files, StringComparer.Ordinal);
        }

        public InMemoryFileManager(params (string Path, string Content)[] files)
            : this(files.ToDictionary(f => f.Path, f => f.Content, StringComparer.Ordinal))
        {
        }

        public Task<string> ReadFileAsync(string path)
        {
            if (!_files.TryGetValue(path, out var content))
                throw new FileNotFoundException($"File not found: {path}");
            return Task.FromResult(content);
        }

        public IEnumerable<string> ExpandFiles(IEnumerable<string> inputs) => _files.Keys.ToList();

        public Task WriteFileAsync(string path, string content)
        {
            _files[path] = content;
            Writes.Add((path, content));
            return Task.CompletedTask;
        }

        public Task WriteFilesToDiskAsync(Dictionary<string, string> results, string outputDirectory)
        {
            foreach (var kv in results)
            {
                var full = Path.Combine(outputDirectory, kv.Key);
                _files[full] = kv.Value;
                Writes.Add((full, kv.Value));
            }
            return Task.CompletedTask;
        }
    }
}
