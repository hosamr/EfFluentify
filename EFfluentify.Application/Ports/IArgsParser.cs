using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Application.Ports
{
    public interface IArgsParser
    {
        (List<string> inputs, string? output, bool manyFiles, bool remove) ParseOrThrow(string[] args);
    }
}
