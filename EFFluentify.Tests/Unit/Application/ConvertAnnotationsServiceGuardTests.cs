using EFFluentify.Application.Services;
using EFFluentify.Domain.Rules;
using EFFluentify.Infrastructure.CodeGen;
using EFFluentify.Infrastructure.IO;
using EFFluentify.Infrastructure.Roslyn;
using Xunit;

namespace EFFluentify.Tests.Unit.Application
{
    [Trait("Category", "Unit")]
    public class ConvertAnnotationsServiceGuardTests
    {
        private static RoslynEntityModelBuilder Builder() => new(new FileManager());

        [Fact]
        public void NullBuilder_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ConvertAnnotationsService(null!, new CSharpConfigEmitter(), new RuleRegistryFactory()));
        }

        [Fact]
        public void NullGenerator_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ConvertAnnotationsService(Builder(), null!, new RuleRegistryFactory()));
        }

        [Fact]
        public void NullRuleRegistryFactory_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ConvertAnnotationsService(Builder(), new CSharpConfigEmitter(), null!));
        }
    }
}
