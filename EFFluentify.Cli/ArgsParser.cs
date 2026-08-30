using EFFluentify.Application.Models;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace EFFluentify.Cli
{
    public sealed class ArgsParser : IArgsParser
    {
        public CommandRequest ParseOrThrow(string[] args)
        {
            var inputOpt = new Option<List<string>>("--input", "One or more input paths")
            {
                IsRequired = true,
                Arity = ArgumentArity.OneOrMore
            };
            var outputOpt = new Option<string?>("--out", "Output directory; when omitted, generated code is written to the console");
            var manyFilesOpt = new Option<bool>("--manyFiles", "Emit one configuration file per entity");
            var removeOpt = new Option<bool>("--removeAnnotationsFromMyOriginal", "Remove the converted annotations from the original source files");
            var namespaceOpt = new Option<string>(new[] { "--namespace", "-n" }, () => "EFFluentify.Configurations", "The root namespace for generated files");

            var root = new RootCommand { inputOpt, outputOpt, manyFilesOpt, removeOpt, namespaceOpt };
            var result = new Parser(root).Parse(args);

            if (result.Errors.Count > 0)
                throw new ArgumentException(string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));

            var output = result.GetValueForOption(outputOpt);

            var options = new PipelineOptions
            {
                OutputDirectory = output ?? ".",
                ManyFiles = result.GetValueForOption(manyFilesOpt),
                RemoveAnnotationsFromOriginal = result.GetValueForOption(removeOpt),
                RootNamespace = result.GetValueForOption(namespaceOpt)!
            };

            return new CommandRequest(
                result.GetValueForOption(inputOpt)!,
                options,
                PrintToConsole: output is null);
        }
    }
}
