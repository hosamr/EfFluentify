using EFfluentify.Application.Interfaces;

namespace EFfluentify.Infrastructure.IO
{
    public class ConsoleManager : IConsoleManager
    {
        public void WriteLine(string message) => Console.WriteLine(message);
        public void WriteError(string message) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ResetColor();
        }
        public string? ReadLine() => Console.ReadLine();
    }
}
