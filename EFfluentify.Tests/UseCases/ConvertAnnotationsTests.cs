using EFfluentify.Application.Interfaces;
using EFfluentify.Application.UseCases;
using EFfluentify.Application.Models;
using EFfluentify.Infrastructure.CodeGen;
using EFfluentify.Infrastructure.IO;
using EFfluentify.Infrastructure.Roslyn;
using EFfluentify.Tests.Helpers;
using Xunit;

namespace EFfluentify.Tests.UseCases
{

    [CollectionDefinition("Sequential Tests", DisableParallelization = true)]
    public class SequentialTestCollection { }

    [Collection("Sequential Tests")]
    public class ConvertAnnotationsTests
    {
        private const string TestCase1 = "TestCase1";
        private const string TestFkCases = "TestFkCases";
        private const string ExpectedOutput = "ExpectedOutput";
        private const string ExpectedOutputRemoval = "ExpectedOutputRemoval";

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task Run_Pipeline_For_TestCase1_With_All_Flag_Combinations_Succeeds(bool removeAnnotations, bool manyFiles)
        {
            var options = new PipelineOptions
            {
                RemoveAnnotationsFromOriginal = removeAnnotations,
                ManyFiles = manyFiles
            };

            await RunTestCaseWithOptionsAsync(
                options,
                TestCase1,
                ExpectedOutput,
                ExpectedOutputRemoval);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public async Task Run_Pipeline_For_TestFkCases(bool removeAnnotations, bool manyFiles)
        {
            var options = new PipelineOptions
            {
                RemoveAnnotationsFromOriginal = removeAnnotations,
                ManyFiles = manyFiles
            };

            await RunTestCaseWithOptionsAsync(
                options,
                TestFkCases,
                ExpectedOutput,
                ExpectedOutputRemoval);
        }
        [Fact]
        public async Task Fails_For_TestCase1_When_OutputDirectory_IsInvalid()
        {
            var root = TestingHelper.FindSolutionRoot();
            var inputDir = Path.Combine(root, "EFfluentify.Tests", "TestData", TestCase1);

            IFileManager fileManager = new FileManager();
            var builder = new RoslynEntityModelBuilder(fileManager);
            var generator = new CSharpConfigEmitter();
            var useCase = new ConvertAnnotationsUseCase(builder, generator);

            var options = new PipelineOptions
            {
                OutputDirectory = @"Z:\This\Path\Cannot\Exist",
                RemoveAnnotationsFromOriginal = false,
                ManyFiles = true
            };

            IEnumerable<string> inputs = new[] { inputDir };

            await Assert.ThrowsAsync<DirectoryNotFoundException>(async () =>
            {
                var results = await useCase.Run(inputs, options);
                await fileManager.writeFilesToDiskAsync(results, options.OutputDirectory);
            });
        }

        [Fact]
        public async Task RegenerateTestFkCases_ExpectedOutput_SingleFile()
        {
            var options = new PipelineOptions
            {
                RemoveAnnotationsFromOriginal = false,
                ManyFiles = false,
                OutputDirectory = @"e:\EfFluentify\EfFluentify\ExpectedOutput\TestFkCasesSingleFile"
            };

            var solutionRoot = TestingHelper.FindSolutionRoot();
            var testProjectRoot = Path.Combine(solutionRoot, "EFfluentify.Tests");
            var inputDir = Path.Combine(testProjectRoot, "TestData", TestFkCases);

            IFileManager fileManager = new FileManager();
            var useCase = CreateUseCase(fileManager);
            IEnumerable<string> inputs = new[] { inputDir };

            var results = await useCase.Run(inputs, options);
            await fileManager.writeFilesToDiskAsync(results, options.OutputDirectory);
        }
        [Fact]
        public async Task TestCase1_When_InputDirectory_IsInvalid_Should_DoNothing()
        {
            string invalidInputDir = @"Z:\Invalid\Path\That\Does\Not\Exist";

            var tempDir = Directory.CreateTempSubdirectory("EFfluentifyTests_").FullName;
            var outputDir = Path.Combine(tempDir, "EFfluentify.Tests", "Output");

            IFileManager fileManager = new FileManager();
            var builder = new RoslynEntityModelBuilder(fileManager);
            var generator = new CSharpConfigEmitter();
            var useCase = new ConvertAnnotationsUseCase(builder, generator);

            var options = new PipelineOptions
            {
                OutputDirectory = outputDir,
                RemoveAnnotationsFromOriginal = true,
                ManyFiles = true
            };

            IEnumerable<string> inputs = new[] { invalidInputDir };

            var results = await useCase.Run(inputs, options);
            Assert.Empty(results);
            Assert.False(Directory.Exists(outputDir), "Output directory should not be created when input directory is invalid.");
        }

        private async Task RunTestCaseWithOptionsAsync(PipelineOptions pipelineOptions, string testcaseName, string expectedOutput, string expectedOutputRemoval)
        {
            var solutionRoot = TestingHelper.FindSolutionRoot();
            var tempDirectory = Directory.CreateTempSubdirectory("EFfluentifyTests_");
            var tempRoot = tempDirectory.FullName;
            var testProjectRoot = Path.Combine(solutionRoot, "EFfluentify.Tests");

            var inputDir = Path.Combine(testProjectRoot, "TestData", testcaseName);
            var outputDir = Path.Combine(tempRoot, "EFfluentify.Tests", "Output");

            var expectedDirectory = @"E:\EfFluentify\EfFluentify";
            var expectedOutputDir = Path.Combine(expectedDirectory, expectedOutput, testcaseName);
            var expectedOutputRemovalDir = Path.Combine(expectedDirectory, expectedOutputRemoval, testcaseName);

            try
            {
                pipelineOptions.OutputDirectory = outputDir;
                IFileManager fileManager = new FileManager();
                var useCase = CreateUseCase(fileManager);
                IEnumerable<string> inputs = new[] { inputDir };

                var results = await useCase.Run(inputs, pipelineOptions);
                await fileManager.writeFilesToDiskAsync(results, outputDir);

                if (pipelineOptions.RemoveAnnotationsFromOriginal)
                {
                    await ApplyAnnotationRemovalAsync(fileManager, inputs);
                }
                Assert.True(Directory.Exists(outputDir), "Output directory was not created.");

                if (pipelineOptions.ManyFiles)
                    await AssertManyFilesMatchAsync(expectedOutputDir, outputDir);

                else
                {
                    var expectedOutputDirSingleFile = expectedOutputDir + "SingleFile";
                    await AssertSingleFileMatchesAsync(expectedOutputDirSingleFile, outputDir);
                }


                if (pipelineOptions.RemoveAnnotationsFromOriginal && Directory.Exists(expectedOutputRemovalDir))
                {
                    await AssertOriginalFilesAndRestoreAsync(expectedOutputRemovalDir, inputDir);
                }

            }
            finally
            {
                try
                {
                    tempDirectory.Delete(recursive: true);
                }
                catch
                {
                }
            }
        }

        private static ConvertAnnotationsUseCase CreateUseCase(IFileManager fileManager)
        {
            var builder = new RoslynEntityModelBuilder(fileManager);
            var generator = new CSharpConfigEmitter();

            return new ConvertAnnotationsUseCase(builder, generator);
        }

        private static async Task AssertManyFilesMatchAsync(string expectedDir, string actualDir)
        {
            foreach (var expectedFile in Directory.EnumerateFiles(expectedDir, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(expectedDir, expectedFile);
                var actualFile = Path.Combine(actualDir, relativePath);

                Assert.True(File.Exists(actualFile), $"Expected generated file not found: {relativePath}");

                var expectedText = await File.ReadAllTextAsync(expectedFile);
                var actualText = await File.ReadAllTextAsync(actualFile);

                Assert.Equal(TestingHelper.Normalize(expectedText), TestingHelper.Normalize(actualText));
            }
        }

        private static async Task AssertOriginalFilesAndRestoreAsync(string expectedRemovalDir, string inputDir)
        {
            foreach (var expectedFile in Directory.EnumerateFiles(expectedRemovalDir, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(expectedRemovalDir, expectedFile);
                var actualFile = Path.Combine(inputDir, relativePath);

                Assert.True(File.Exists(actualFile), $"Expected modified input file not found: {relativePath}");

                var expectedText = await File.ReadAllTextAsync(expectedFile);
                var actualText = await File.ReadAllTextAsync(actualFile);

                Assert.Equal(TestingHelper.Normalize(expectedText), TestingHelper.Normalize(actualText));

                File.Delete(actualFile);
                File.Move(Path.Combine(inputDir, relativePath + ".bak"), actualFile);
            }
        }

        private async Task AssertSingleFileMatchesAsync(string expectedOutputDir, string outputDir)
        {
            var expectedFiles = Directory.GetFiles(expectedOutputDir, "*.*", SearchOption.TopDirectoryOnly);
            Assert.Single(expectedFiles);

            var expectedFile = expectedFiles[0];

            var actualFiles = Directory.GetFiles(outputDir, "*.*", SearchOption.TopDirectoryOnly);
            Assert.Single(actualFiles);

            var actualFile = actualFiles[0];

            var expectedText = await File.ReadAllTextAsync(expectedFile);
            var actualText = await File.ReadAllTextAsync(actualFile);

            Assert.Equal(TestingHelper.Normalize(expectedText), TestingHelper.Normalize(actualText));
        }

        private async Task ApplyAnnotationRemovalAsync(IFileManager fileManager, IEnumerable<string> inputs)
        {
            var annotationRemover = new AnnotationRemover(fileManager);
            var changes = await annotationRemover.PrepareRemovalAsync(inputs);

            foreach (var change in changes)
            {
                var backupPath = change.FilePath + ".bak";

                await fileManager.WriteFileAsync(backupPath, change.OriginalContent);
                await fileManager.WriteFileAsync(change.FilePath, change.UpdatedContent);
            }
        }



    }
}