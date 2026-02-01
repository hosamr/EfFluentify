using EFfluentify.Application.Ports;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace EFfluentify.Application.Helpers
{
    public class ArgsParser : IArgsParser
    {
        public (List<string> inputs, string? output, bool manyFiles, bool remove) ParseOrThrow(string[] args)
        {
            var inputOpt = new Option<List<string>>(name: "--input", description: "One or more input paths")
            {
                IsRequired = true,
                Arity = ArgumentArity.OneOrMore
            };

            var outputOpt = new Option<string>("--out") { IsRequired = true };
            var manyFilesOpt = new Option<bool>("--manyFiles");
            var removeOpt = new Option<bool>("--removeAnnotationsFromMyOriginal");


            var root = new RootCommand { inputOpt, outputOpt, manyFilesOpt, removeOpt };
            var parser = new Parser(root);

            var result = parser.Parse(args);

            List<string> inputs = result.GetValueForOption(inputOpt)!;
            string output = result.GetValueForOption(outputOpt)!;
            bool manyFiles = result.GetValueForOption(manyFilesOpt);
            bool remove = result.GetValueForOption(removeOpt);
            
            if (result.UnmatchedTokens.Count > 0)
                throw new ArgumentException($"Unrecognized arguments: {string.Join(" ", result.UnmatchedTokens)}");

            return (inputs,output,manyFiles, remove);

        }
    }
}
