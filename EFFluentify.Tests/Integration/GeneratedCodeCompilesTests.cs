using EFFluentify.Application.Interfaces;
using EFFluentify.Application.Models;
using EFFluentify.Application.Services;
using EFFluentify.Domain.Rules;
using EFFluentify.Infrastructure.CodeGen;
using EFFluentify.Infrastructure.IO;
using EFFluentify.Infrastructure.Roslyn;
using EFFluentify.Tests.Helpers;
using Xunit;

namespace EFFluentify.Tests.Integration
{
    [Collection("Sequential Tests")]
    [Trait("Category", "Integration")]
    public class GeneratedCodeCompilesTests
    {
        [Theory]
        [InlineData("TestCase1", true)]
        [InlineData("TestCase1", false)]
        [InlineData("TestFkCases", true)]
        [InlineData("TestFkCases", false)]
        [InlineData("TestScalarAnnotations", true)]
        [InlineData("TestScalarAnnotations", false)]
        [InlineData("TestNullability", true)]
        [InlineData("TestNullability", false)]
        [InlineData("TestExplicitRelationships", true)]
        [InlineData("TestExplicitRelationships", false)]
        [InlineData("TestIndexesAndColumns", true)]
        [InlineData("TestIndexesAndColumns", false)]
        public async Task Generated_Configuration_Compiles_Against_Input_Entities(
            string testCaseName, bool manyFiles)
        {
            var solutionRoot = TestingHelper.FindSolutionRoot();
            var inputDir = Path.Combine(
                solutionRoot, "EFFluentify.Tests", "TestData", testCaseName);

            IFileManager fileManager = new FileManager();
            var useCase = new ConvertAnnotationsService(
                new RoslynEntityModelBuilder(fileManager),
                new CSharpConfigEmitter(),
                new RuleRegistryFactory());

            var options = new PipelineOptions
            {
                RemoveAnnotationsFromOriginal = false,
                ManyFiles = manyFiles,
            };

            var generated = await useCase.RunAsync(new[] { inputDir }, options);
            Assert.NotEmpty(generated);

            var entitySources = Directory
                .EnumerateFiles(inputDir, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .ToList();

            var result = GeneratedCodeCompiler.Compile(entitySources, generated.Values);

            Assert.True(
                result.Success,
                $"Generated configuration for '{testCaseName}' " +
                $"(manyFiles={manyFiles}) did not compile:{result.FormatErrors()}");
        }
    }
}
