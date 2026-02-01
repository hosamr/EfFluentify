using EFfluentify.Application.Ports;
using EFfluentify.Domain.Rules;

namespace EFfluentify.Application.UseCases
{
    public sealed class ConvertAnnotationsUseCase
    {
        private readonly IEntityModelBuilder _builder;
        private readonly ICodeGenerator _generator;
        public ConvertAnnotationsUseCase(
            IEntityModelBuilder builder,
            ICodeGenerator generator)
        {
            _builder = builder;
            _generator = generator;
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
