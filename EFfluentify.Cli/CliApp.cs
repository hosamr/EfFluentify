using EFfluentify.Application.Interfaces;
using EFfluentify.Application.Services;
using EFfluentify.Application.Models;
public class CliApp
{
    private readonly IConvertAnnotationsService _convertAnnotationsService;
    private readonly IConsoleManager _console;
    private readonly IArgsParser _argsParser;
    private readonly IFileManager _fileSystemService;
    private readonly IAnnotationRemover _annotationRemover;
    public CliApp(
        IConvertAnnotationsService convertAnnotationsService,
        IConsoleManager console,
        IArgsParser argsParser,
        IFileManager fileSystemService,
        IAnnotationRemover annotationRemover)
    {
        _convertAnnotationsService = convertAnnotationsService ?? throw new ArgumentNullException(nameof(convertAnnotationsService));
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _argsParser = argsParser ?? throw new ArgumentNullException(nameof(argsParser));
        _fileSystemService = fileSystemService ?? throw new ArgumentNullException(nameof(fileSystemService));
        _annotationRemover = annotationRemover ?? throw new ArgumentNullException(nameof(annotationRemover));
    }

    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            var commandArgs = GetOrPromptForArgs(args);
            var (inputs, output, manyFiles, remove, rootNamespace) = _argsParser.ParseOrThrow(commandArgs);

            var options = new PipelineOptions
            {
                OutputDirectory = output!,
                ManyFiles = manyFiles,
                RemoveAnnotationsFromOriginal = remove,
                RootNamespace = rootNamespace
            };

            var results = await _convertAnnotationsService.Run(inputs, options);
            await HandleOutputAsync(results, output, options.OutputDirectory);

            if (options.RemoveAnnotationsFromOriginal)
            {
                await RemoveAnnotationsAsync(inputs);
            }

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

    private async Task HandleOutputAsync(Dictionary<string, string> results, string? output, string outputDirectory)
    {
        if (output == null)
        {
            foreach (var kv in results)
            {
                _console.WriteLine($"// File: {kv.Key}");
                _console.WriteLine(kv.Value);
            }
        }
        else
        {
            await _fileSystemService.WriteFilesToDiskAsync(results, outputDirectory);
        }
    }

    private async Task RemoveAnnotationsAsync(IEnumerable<string> inputs)
    {
        var changes = await _annotationRemover.PrepareRemovalAsync(inputs);
        foreach (var change in changes)
        {
            var bak = change.FilePath + ".bak";
            await _fileSystemService.WriteFileAsync(bak, change.OriginalContent);
            await _fileSystemService.WriteFileAsync(change.FilePath, change.UpdatedContent);
        }
    }
}