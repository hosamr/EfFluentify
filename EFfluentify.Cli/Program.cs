using Microsoft.Extensions.DependencyInjection;

namespace EFfluentify.Cli
{
    internal static class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try
            {
                using var provider = new ServiceCollection()
                    .AddEFfluentify()
                    .BuildServiceProvider();

                var app = provider.GetRequiredService<CliApp>();
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
