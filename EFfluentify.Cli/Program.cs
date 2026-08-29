using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EFfluentify.Cli
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try
            {
                using var host = Host.CreateDefaultBuilder(args).ConfigureServices((ctx, services) =>
                {
                    services.AddEFfluentify();

                }).Build();

                var app = host.Services.GetRequiredService<CliApp>();
                return await app.RunAsync(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"Fatal error: {ex}");
                Console.ResetColor();
                return 1;
            }
        }
    }
}
