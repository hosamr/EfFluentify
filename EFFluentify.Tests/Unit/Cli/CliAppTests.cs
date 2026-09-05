using EFFluentify.Application.Interfaces;
using EFFluentify.Application.Models;
using EFFluentify.Cli;
using EFFluentify.Tests.Helpers;
using Xunit;

namespace EFFluentify.Tests.Unit.Cli
{
    [Trait("Category", "Unit")]
    public class CliAppTests
    {
        private sealed class FakeConsole : IConsoleManager
        {
            public List<string> Lines { get; } = new();
            public List<string> Errors { get; } = new();
            public Queue<string?> ReadLines { get; } = new();

            public void WriteLine(string message) => Lines.Add(message);
            public void WriteError(string message) => Errors.Add(message);
            public string? ReadLine() => ReadLines.Count > 0 ? ReadLines.Dequeue() : null;
        }

        private sealed class FakeArgsParser : IArgsParser
        {
            private readonly CommandRequest? _request;
            private readonly Exception? _throw;
            public string[]? ReceivedArgs { get; private set; }

            public FakeArgsParser(CommandRequest? request = null, Exception? toThrow = null)
            {
                _request = request;
                _throw = toThrow;
            }

            public CommandRequest ParseOrThrow(string[] args)
            {
                ReceivedArgs = args;
                if (_throw != null) throw _throw;
                return _request!;
            }
        }

        private sealed class FakeConvertService : IConvertAnnotationsService
        {
            private readonly Dictionary<string, string> _result;
            public FakeConvertService(Dictionary<string, string> result) => _result = result;
            public Task<Dictionary<string, string>> RunAsync(IEnumerable<string> inputs, PipelineOptions options)
                => Task.FromResult(_result);
        }

        private sealed class FakeAnnotationRemover : IAnnotationRemover
        {
            private readonly IReadOnlyList<AnnotationRemovalChange> _changes;
            public IEnumerable<string>? ReceivedInputs { get; private set; }
            public FakeAnnotationRemover(params AnnotationRemovalChange[] changes) => _changes = changes;

            public Task<IReadOnlyList<AnnotationRemovalChange>> PrepareRemovalAsync(IEnumerable<string> inputs)
            {
                ReceivedInputs = inputs;
                return Task.FromResult(_changes);
            }
        }

        private static CommandRequest Request(PipelineOptions options, bool printToConsole, params string[] inputs)
            => new(inputs, options, printToConsole);

        [Fact]
        public async Task PrintToConsole_WritesFilesToConsole_NotDisk()
        {
            var console = new FakeConsole();
            var fileManager = new InMemoryFileManager();
            var results = new Dictionary<string, string> { ["UserConfiguration.cs"] = "// config" };

            var app = new CliApp(
                new FakeConvertService(results), console,
                new FakeArgsParser(Request(new PipelineOptions(), printToConsole: true, "src")),
                fileManager, new FakeAnnotationRemover());

            var exit = await app.RunAsync(new[] { "--input", "src" });

            Assert.Equal(0, exit);
            Assert.Contains("// File: UserConfiguration.cs", console.Lines);
            Assert.Contains("// config", console.Lines);
            Assert.Empty(fileManager.Writes);
        }

        [Fact]
        public async Task WriteToDisk_WritesToOutputDirectory()
        {
            var fileManager = new InMemoryFileManager();
            var results = new Dictionary<string, string> { ["UserConfiguration.cs"] = "// config" };
            var options = new PipelineOptions { OutputDirectory = "out" };

            var app = new CliApp(
                new FakeConvertService(results), new FakeConsole(),
                new FakeArgsParser(Request(options, printToConsole: false, "src")),
                fileManager, new FakeAnnotationRemover());

            var exit = await app.RunAsync(new[] { "--input", "src", "--out", "out" });

            Assert.Equal(0, exit);
            Assert.Contains(fileManager.Writes, w => w.Path == Path.Combine("out", "UserConfiguration.cs") && w.Content == "// config");
        }

        [Fact]
        public async Task RemoveAnnotations_WritesBackupAndUpdatedFiles()
        {
            var fileManager = new InMemoryFileManager();
            var options = new PipelineOptions { OutputDirectory = "out", RemoveAnnotationsFromOriginal = true };
            var change = new AnnotationRemovalChange("Model.cs", "ORIGINAL", "UPDATED");

            var app = new CliApp(
                new FakeConvertService(new Dictionary<string, string>()), new FakeConsole(),
                new FakeArgsParser(Request(options, printToConsole: false, "src")),
                fileManager, new FakeAnnotationRemover(change));

            var exit = await app.RunAsync(new[] { "--input", "src", "--removeAnnotationsFromMyOriginal" });

            Assert.Equal(0, exit);
            Assert.Contains(fileManager.Writes, w => w.Path == "Model.cs.bak" && w.Content == "ORIGINAL");
            Assert.Contains(fileManager.Writes, w => w.Path == "Model.cs" && w.Content == "UPDATED");
        }

        [Fact]
        public async Task ParserThrows_ReturnsOne_AndReportsError()
        {
            var console = new FakeConsole();
            var app = new CliApp(
                new FakeConvertService(new Dictionary<string, string>()), console,
                new FakeArgsParser(toThrow: new ArgumentException("bad args")),
                new InMemoryFileManager(), new FakeAnnotationRemover());

            var exit = await app.RunAsync(new[] { "--nope" });

            Assert.Equal(1, exit);
            Assert.NotEmpty(console.Errors);
        }

        [Fact]
        public async Task NoArgs_PromptsConsole_AndForwardsSplitInput()
        {
            var console = new FakeConsole();
            console.ReadLines.Enqueue("--input src");
            var parser = new FakeArgsParser(Request(new PipelineOptions(), printToConsole: true, "src"));

            var app = new CliApp(
                new FakeConvertService(new Dictionary<string, string>()), console,
                parser, new InMemoryFileManager(), new FakeAnnotationRemover());

            await app.RunAsync(Array.Empty<string>());

            Assert.Equal(new[] { "--input", "src" }, parser.ReceivedArgs);
        }

        [Fact]
        public async Task NoArgs_WithNoConsoleInput_ForwardsEmptyArgs()
        {
            var console = new FakeConsole(); // ReadLine returns null when the queue is empty
            var parser = new FakeArgsParser(Request(new PipelineOptions(), printToConsole: true, "src"));

            var app = new CliApp(
                new FakeConvertService(new Dictionary<string, string>()), console,
                parser, new InMemoryFileManager(), new FakeAnnotationRemover());

            await app.RunAsync(Array.Empty<string>());

            Assert.Equal(Array.Empty<string>(), parser.ReceivedArgs);
        }
    }
}
