using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules;
using EFfluentify.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Application.Interfaces
{
    public interface ICodeGenerator
    {
        Dictionary<string, string> Generate(IEnumerable<EntityModel> entities, PipelineOptions options, RuleRegistry rules);
    }

}
