using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Application.Ports
{
    public interface IConsoleManager
    {
        void WriteLine(string message);
        void WriteError(string message);
        string? ReadLine();
    }
}
