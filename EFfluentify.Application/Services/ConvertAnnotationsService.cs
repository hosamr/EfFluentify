using EFfluentify.Application.Interfaces;
using EFfluentify.Application.Models;
using EFfluentify.Domain.Rules;

namespace EFfluentify.Application.Services
{
    public sealed class ConvertAnnotationsService : IConvertAnnotationsService
    {
        private readonly IEntityModelBuilder _builder;
        private readonly ICodeGenerator _generator;
        public ConvertAnnotationsService(
            IEntityModelBuilder builder,
            ICodeGenerator generator)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        }

        public async Task<Dictionary<string, string>> Run(IEnumerable<string> inputs, PipelineOptions options)
        {
            var entities = await _builder.BuildFromInputs(inputs);
            var registry = RuleRegistry.Default(entities);
            var files = _generator.Generate(entities, options,registry);
            return files;
        }
    }
}
