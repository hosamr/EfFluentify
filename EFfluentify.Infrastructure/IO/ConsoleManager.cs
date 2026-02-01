using EFfluentify.Application.Ports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Infrastructure.IO
{
    public class ConsoleManager : IConsoleManager
    {
        public void WriteLine(string message) => Console.WriteLine(message);
        public void WriteError(string message) => Console.Error.WriteLine(message);
        public string? ReadLine() => Console.ReadLine();
    }
}
