using EFfluentify.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Application.Ports
{
    public interface IEntityModelBuilder
    {
        Task<List<EntityModel>> BuildFromInputs(IEnumerable<string> inputs);
    }
}
