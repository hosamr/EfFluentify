using EFfluentify.Application.Ports;
using EFfluentify.Application.UseCases;
using EFfluentify.Domain.Rules;

public class CliApp
{
    private readonly ConvertAnnotationsUseCase _useCase;
    private readonly IConsoleManager _console;
    private readonly IArgsParser _argsParser;
    private readonly IFileManager _fileSystemService;
    private readonly IAnnotationRemover _annotationRemover;
    public CliApp(
        ConvertAnnotationsUseCase useCase,
        IConsoleManager console,
        IArgsParser argsParser,
        IFileManager fileSystemService,
        IAnnotationRemover annotationRemover)
    {
        _useCase = useCase;
        _console = console;
        _argsParser = argsParser;
        _fileSystemService = fileSystemService;
        _annotationRemover = annotationRemover;
    }

    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                _console.WriteLine("Enter command: ");
                var input = _console.ReadLine();
                args = input!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
            var (inputs, output, manyFiles, remove) = _argsParser.ParseOrThrow(args);

            var options = new PipelineOptions
            {
                OutputDirectory = output!,
                ManyFiles = manyFiles,
                RemoveAnnotationsFromOriginal = remove,
                RootNamespace = "EFfluentify.Configurations"
            };

            var results = await _useCase.Run(inputs, options);
            if(output == null)
            {
                foreach (var kv in results)
                {
                    Console.WriteLine($"// File: {kv.Key}");
                    Console.WriteLine(kv.Value);
                    Console.WriteLine();
                }
                return 0;
            }
            await _fileSystemService.writeFilesToDiskAsync(results, output);

            if (options.RemoveAnnotationsFromOriginal)
            {
                var registry = RuleRegistry.Default();
                var changes = await _annotationRemover.PrepareRemovalAsync(inputs);
                foreach (var change in changes)
                {
                    var bak = change.FilePath + ".bak";
                    await _fileSystemService.WriteFileAsync(bak, change.OriginalContent);

                    await _fileSystemService.WriteFileAsync(change.FilePath, change.UpdatedContent);
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(ex.ToString());
            Console.ResetColor();
            return 1;
        }
    }
}
