using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Domain.Models
{
    public sealed class AttributeModel
    {
        public string Name { get; set; } = "";
        public List<string> PositionalArgs { get; set; } = new();
        public Dictionary<string, string> NamedArgs { get; set; } = new();
        public string RawText { get; set; } = "";
    }

}
