using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Application.Ports
{
    public interface IFileManager
    {
        Task<string> ReadFileAsync(string path);
        Task writeFilesToDiskAsync(Dictionary<string, string> results, string OutputDirectory);
        IEnumerable<string> ExpandFiles(IEnumerable<string> inputs);
        Task WriteFileAsync(string path, string content);
    }
}
