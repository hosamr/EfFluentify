using EFfluentify.Application.Models;
using EFfluentify.Domain.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFfluentify.Application.Ports
{
    public interface IAnnotationRemover
    {
        Task<IReadOnlyList<AnnotationRemovalChange>> PrepareRemovalAsync(IEnumerable<string> inputs);
    }
}
