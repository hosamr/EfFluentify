using EFFluentify.Application.Models;
using System.CommandLine;

namespace EFFluentify.Cli
{
    public sealed class ArgsParser : IArgsParser
    {
        public CommandRequest ParseOrThrow(string[] args)
        {
            var inputOpt = new Option<List<string>>("--input")
            {
                Description = "One or more input paths",
                Required = true,
                Arity = ArgumentArity.OneOrMore
            };
            var outputOpt = new Option<string?>("--out")
            {
                Description = "Output directory; when omitted, generated code is written to the console"
            };
            var manyFilesOpt = new Option<bool>("--manyFiles")
            {
                Description = "Emit one configuration file per entity"
            };
            var removeOpt = new Option<bool>("--removeAnnotationsFromMyOriginal")
            {
                Description = "Remove the converted annotations from the original source files"
            };
            var namespaceOpt = new Option<string>("--namespace", "-n")
            {
                Description = "The root namespace for generated files",
                DefaultValueFactory = _ => "EFFluentify.Configurations"
            };

            var root = new RootCommand { inputOpt, outputOpt, manyFilesOpt, removeOpt, namespaceOpt };
            var result = root.Parse(args);

            if (result.Errors.Count > 0)
                throw new ArgumentException(string.Join(Environment.NewLine, result.Errors.Select(e => e.Message)));

            var output = result.GetValue(outputOpt);

            var options = new PipelineOptions
            {
                OutputDirectory = output ?? ".",
                ManyFiles = result.GetValue(manyFilesOpt),
                RemoveAnnotationsFromOriginal = result.GetValue(removeOpt),
                RootNamespace = result.GetValue(namespaceOpt)!
            };

            return new CommandRequest(
                result.GetValue(inputOpt)!,
                options,
                PrintToConsole: output is null);
        }
    }
}
