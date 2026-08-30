using EFFluentify.Application.Interfaces;

namespace EFFluentify.Cli
{
    public sealed class CliApp
    {
        private readonly IConvertAnnotationsService _convertAnnotationsService;
        private readonly IConsoleManager _console;
        private readonly IArgsParser _argsParser;
        private readonly IFileManager _fileManager;
        private readonly IAnnotationRemover _annotationRemover;

        public CliApp(
            IConvertAnnotationsService convertAnnotationsService,
            IConsoleManager console,
            IArgsParser argsParser,
            IFileManager fileManager,
            IAnnotationRemover annotationRemover)
        {
            _convertAnnotationsService = convertAnnotationsService ?? throw new ArgumentNullException(nameof(convertAnnotationsService));
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _argsParser = argsParser ?? throw new ArgumentNullException(nameof(argsParser));
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _annotationRemover = annotationRemover ?? throw new ArgumentNullException(nameof(annotationRemover));
        }

        public async Task<int> RunAsync(string[] args)
        {
            try
            {
                var request = _argsParser.ParseOrThrow(GetOrPromptForArgs(args));

                var results = await _convertAnnotationsService.RunAsync(request.Inputs, request.Options);

                if (request.PrintToConsole)
                    WriteResultsToConsole(results);
                else
                    await _fileManager.WriteFilesToDiskAsync(results, request.Options.OutputDirectory);

                if (request.Options.RemoveAnnotationsFromOriginal)
                    await RemoveAnnotationsAsync(request.Inputs);

                return 0;
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
                return 1;
            }
        }

        private string[] GetOrPromptForArgs(string[] args)
        {
            if (args.Length > 0)
                return args;

            _console.WriteLine("Enter command: ");
            var input = _console.ReadLine();
            return input?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        }

        private void WriteResultsToConsole(Dictionary<string, string> results)
        {
            foreach (var file in results)
            {
                _console.WriteLine($"// File: {file.Key}");
                _console.WriteLine(file.Value);
            }
        }

        private async Task RemoveAnnotationsAsync(IEnumerable<string> inputs)
        {
            var changes = await _annotationRemover.PrepareRemovalAsync(inputs);
            foreach (var change in changes)
            {
                var backupPath = change.FilePath + ".bak";
                await _fileManager.WriteFileAsync(backupPath, change.OriginalContent);
                await _fileManager.WriteFileAsync(change.FilePath, change.UpdatedContent);
            }
        }
    }
}
