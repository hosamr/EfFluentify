using EFFluentify.Application.Interfaces;
using EFFluentify.Application.Models;
using EFFluentify.Domain.Rules.Interfaces;

namespace EFFluentify.Application.Services
{
    public sealed class ConvertAnnotationsService : IConvertAnnotationsService
    {
        private readonly IEntityModelBuilder _builder;
        private readonly ICodeGenerator _generator;
        private readonly IRuleRegistryFactory _ruleRegistryFactory;

        public ConvertAnnotationsService(
            IEntityModelBuilder builder,
            ICodeGenerator generator,
            IRuleRegistryFactory ruleRegistryFactory)
        {
            _builder = builder ?? throw new ArgumentNullException(nameof(builder));
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _ruleRegistryFactory = ruleRegistryFactory ?? throw new ArgumentNullException(nameof(ruleRegistryFactory));
        }

        public async Task<Dictionary<string, string>> RunAsync(IEnumerable<string> inputs, PipelineOptions options)
        {
            var entities = await _builder.BuildFromInputsAsync(inputs);
            var registry = _ruleRegistryFactory.Create(entities);
            var files = _generator.Generate(entities, options, registry);
            return files;
        }
    }
}
