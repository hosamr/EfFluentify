using EFFluentify.Infrastructure.IO;
using Xunit;

namespace EFFluentify.Tests.Integration
{
    [Trait("Category", "Integration")]
    [Collection("Sequential Tests")]
    public class FileManagerTests : IDisposable
    {
        private readonly string _root;

        public FileManagerTests()
        {
            _root = Directory.CreateTempSubdirectory("EFFluentifyFmTests_").FullName;
        }

        public void Dispose()
        {
            try { Directory.Delete(_root, recursive: true); } catch { }
        }

        private string Touch(string relative, string content = "// x")
        {
            var full = Path.Combine(_root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(full)!);
            File.WriteAllText(full, content);
            return full;
        }

        [Fact]
        public async Task ReadFileAsync_Missing_Throws()
        {
            var fm = new FileManager();
            await Assert.ThrowsAsync<FileNotFoundException>(() => fm.ReadFileAsync(Path.Combine(_root, "nope.cs")));
        }

        [Fact]
        public async Task WriteFileAsync_Then_ReadFileAsync_RoundTrips()
        {
            var fm = new FileManager();
            var path = Path.Combine(_root, "sub", "a.cs");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await fm.WriteFileAsync(path, "hello");
            Assert.Equal("hello", await fm.ReadFileAsync(path));
        }

        [Fact]
        public async Task WriteFilesToDiskAsync_CreatesDirectoryAndFiles()
        {
            var fm = new FileManager();
            var outDir = Path.Combine(_root, "out");
            var results = new Dictionary<string, string>
            {
                ["AConfiguration.cs"] = "// A",
                ["BConfiguration.cs"] = "// B",
            };

            await fm.WriteFilesToDiskAsync(results, outDir);

            Assert.True(File.Exists(Path.Combine(outDir, "AConfiguration.cs")));
            Assert.Equal("// B", await File.ReadAllTextAsync(Path.Combine(outDir, "BConfiguration.cs")));
        }

        [Fact]
        public void ExpandFiles_Directory_ReturnsOnlyCSharpFilesRecursively()
        {
            Touch("a.cs");
            Touch("nested/b.cs");
            Touch("notes.txt");

            var fm = new FileManager();
            var files = fm.ExpandFiles(new[] { _root }).ToList();

            Assert.Equal(2, files.Count);
            Assert.All(files, f => Assert.EndsWith(".cs", f));
            Assert.All(files, f => Assert.True(Path.IsPathFullyQualified(f)));
        }

        [Fact]
        public void ExpandFiles_DeduplicatesRepeatedInputs()
        {
            Touch("a.cs");

            var fm = new FileManager();
            var files = fm.ExpandFiles(new[] { _root, _root }).ToList();

            Assert.Single(files);
        }

        [Fact]
        public void ExpandFiles_SingleFileInput_IsReturned()
        {
            var file = Touch("solo.cs");

            var fm = new FileManager();
            Assert.Equal(new[] { Path.GetFullPath(file) }, fm.ExpandFiles(new[] { file }).ToArray());
        }

        [Fact]
        public void ExpandFiles_NonexistentInput_ReturnsEmpty()
        {
            var fm = new FileManager();
            Assert.Empty(fm.ExpandFiles(new[] { Path.Combine(_root, "ghost") }));
        }
    }
}
