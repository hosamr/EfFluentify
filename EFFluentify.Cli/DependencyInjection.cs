using EFFluentify.Application.Interfaces;
using EFFluentify.Application.Services;
using EFFluentify.Domain.Rules;
using EFFluentify.Domain.Rules.Interfaces;
using EFFluentify.Infrastructure.CodeGen;
using EFFluentify.Infrastructure.IO;
using EFFluentify.Infrastructure.Roslyn;
using Microsoft.Extensions.DependencyInjection;

namespace EFFluentify.Cli
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEFFluentify(this IServiceCollection services)
        {
            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<IConsoleManager, ConsoleManager>();
            services.AddTransient<IEntityModelBuilder, RoslynEntityModelBuilder>();
            services.AddSingleton<ICodeGenerator, CSharpConfigEmitter>();
            services.AddTransient<IAnnotationRemover, AnnotationRemover>();
            services.AddTransient<IArgsParser, ArgsParser>();
            services.AddSingleton<IRuleRegistryFactory, RuleRegistryFactory>();
            services.AddTransient<IConvertAnnotationsService, ConvertAnnotationsService>();
            services.AddSingleton<CliApp>();

            return services;
        }
    }
}
