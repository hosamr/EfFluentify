using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Application.Ports
{
    public interface ICodeGenerator
    {
        Dictionary<string, string> Generate(IEnumerable<EntityModel> entities, PipelineOptions options, RuleRegistry rules);
    }

}
