using EFFluentify.Application.Interfaces;
using EFFluentify.Cli;
using EFFluentify.Domain.Rules.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EFFluentify.Tests.Integration
{
    [Trait("Category", "Integration")]
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddEFFluentify_ResolvesCliApp_AndEveryDependency()
        {
            using var provider = new ServiceCollection().AddEFFluentify().BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<CliApp>());
            Assert.NotNull(provider.GetRequiredService<IConvertAnnotationsService>());
            Assert.NotNull(provider.GetRequiredService<IFileManager>());
            Assert.NotNull(provider.GetRequiredService<IConsoleManager>());
            Assert.NotNull(provider.GetRequiredService<IArgsParser>());
            Assert.NotNull(provider.GetRequiredService<IAnnotationRemover>());
            Assert.NotNull(provider.GetRequiredService<IEntityModelBuilder>());
            Assert.NotNull(provider.GetRequiredService<ICodeGenerator>());
            Assert.NotNull(provider.GetRequiredService<IRuleRegistryFactory>());
        }
    }
}
