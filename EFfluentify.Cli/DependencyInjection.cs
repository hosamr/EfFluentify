using EFfluentify.Application.Helpers;
using EFfluentify.Application.Interfaces;
using EFfluentify.Application.Services;
using EFfluentify.Domain.Rules;
using EFfluentify.Infrastructure.CodeGen;
using EFfluentify.Infrastructure.IO;
using EFfluentify.Infrastructure.Roslyn;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EFfluentify.Cli
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddEFfluentify(this IServiceCollection services) { 

            services.AddLogging(b => b.AddConsole());

            services.AddSingleton<IFileManager, FileManager>();
            services.AddSingleton<IConsoleManager, ConsoleManager>();
            services.AddTransient<IEntityModelBuilder, RoslynEntityModelBuilder>();
            services.AddSingleton<ICodeGenerator, CSharpConfigEmitter>();
            services.AddTransient<IAnnotationRemover, AnnotationRemover>();
            services.AddTransient<IArgsParser, ArgsParser>();
            services.AddSingleton<CliApp>(); 

            services.AddTransient<IConvertAnnotationsService, ConvertAnnotationsService>();

            return services;
        }

    }
}
