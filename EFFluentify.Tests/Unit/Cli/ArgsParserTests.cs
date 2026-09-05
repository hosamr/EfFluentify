using EFFluentify.Cli;
using Xunit;

namespace EFFluentify.Tests.Unit.Cli
{
    [Trait("Category", "Unit")]
    public class ArgsParserTests
    {
        private static CommandRequest Parse(params string[] args) => new ArgsParser().ParseOrThrow(args);

        [Fact]
        public void Defaults_WhenOnlyInputProvided()
        {
            var request = Parse("--input", "src");

            Assert.Equal(new[] { "src" }, request.Inputs.ToArray());
            Assert.Equal(".", request.Options.OutputDirectory);
            Assert.False(request.Options.ManyFiles);
            Assert.False(request.Options.RemoveAnnotationsFromOriginal);
            Assert.Equal("EFFluentify.Configurations", request.Options.RootNamespace);
            Assert.True(request.PrintToConsole);
        }

        [Fact]
        public void Out_SetsOutputDirectory_AndDisablesConsole()
        {
            var request = Parse("--input", "src", "--out", "generated");

            Assert.Equal("generated", request.Options.OutputDirectory);
            Assert.False(request.PrintToConsole);
        }

        [Fact]
        public void Input_AcceptsRepeatedFlags()
        {
            var request = Parse("--input", "a", "--input", "b", "--input", "c");
            Assert.Equal(new[] { "a", "b", "c" }, request.Inputs.ToArray());
        }

        [Fact]
        public void Input_SpaceSeparatedValues_AreNotSupported()
        {
            // The option is OneOrMore but AllowMultipleArgumentsPerToken is not enabled, so extra
            // space-separated tokens are treated as unrecognized. Pins current behavior; if the
            // parser is later configured to accept "--input a b c", update this test.
            Assert.Throws<ArgumentException>(() => Parse("--input", "a", "b", "c"));
        }

        [Fact]
        public void ManyFiles_Flag_SetsOption()
        {
            Assert.True(Parse("--input", "src", "--manyFiles").Options.ManyFiles);
        }

        [Fact]
        public void RemoveAnnotations_Flag_SetsOption()
        {
            Assert.True(Parse("--input", "src", "--removeAnnotationsFromMyOriginal").Options.RemoveAnnotationsFromOriginal);
        }

        [Theory]
        [InlineData("--namespace")]
        [InlineData("-n")]
        public void Namespace_Option_And_Alias(string flag)
        {
            Assert.Equal("Custom.Ns", Parse("--input", "src", flag, "Custom.Ns").Options.RootNamespace);
        }

        [Fact]
        public void MissingInput_Throws()
        {
            Assert.Throws<ArgumentException>(() => Parse("--out", "generated"));
        }
    }
}
