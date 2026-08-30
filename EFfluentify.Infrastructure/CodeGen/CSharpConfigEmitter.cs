using System.Text;
using EFfluentify.Application.Interfaces;
using EFfluentify.Application.Models;
using EFfluentify.Domain.Helpers;
using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Helpers;
using EFfluentify.Domain.Rules.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace EFfluentify.Infrastructure.CodeGen
{
    public sealed class CSharpConfigEmitter : ICodeGenerator
    {
        private IRuleRegistry _rules;
        private IReadOnlyList<EntityModel> _entities = [];
        private IReadOnlySet<string> _entityNames = new HashSet<string>();

        public CSharpConfigEmitter()
        {
            _entities = [];
            _rules = default!;
        }

        public Dictionary<string, string> Generate(IEnumerable<EntityModel> entities, PipelineOptions options, IRuleRegistry rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _entities = entities?.ToList() ?? throw new ArgumentNullException(nameof(entities));
            _entityNames = EfTypeHelper.BuildEntityNameSet(_entities);

            return options.ManyFiles 
                ? GenerateMultipleFiles(options.RootNamespace) 
                : GenerateSingleFile(options.RootNamespace);
        }

        private Dictionary<string, string> GenerateMultipleFiles(string rootNs)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var e in _entities)
            {
                var sb = AddHeader(rootNs);
                AppendConfig(sb, e);
                map[$"{e.Name}Configuration.cs"] = Format(sb.ToString());
            }

            return map;
        }

        private Dictionary<string, string> GenerateSingleFile(string rootNs)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var sb = AddHeader(rootNs);
            
            foreach (var e in _entities)
            {
                AppendConfig(sb, e);
                sb.AppendLine();
            }
            
            map["EntityConfigurations.cs"] = Format(sb.ToString());
            return map;
        }

        private static string Format(string source)
        {
            var root = CSharpSyntaxTree.ParseText(source).GetRoot();

            if (root.GetDiagnostics().Any(d => d.Severity == DiagnosticSeverity.Error))
                return source;

            return root
                .NormalizeWhitespace(indentation: "    ", eol: Environment.NewLine)
                .ToFullString();
        }

        private void AppendConfig(StringBuilder sb, EntityModel entity)
        {
            sb.AppendLine($"internal sealed class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>");
            sb.AppendLine("{");
            sb.AppendLine($"    public void Configure(EntityTypeBuilder<{entity.Name}> builder)");
            sb.AppendLine("    {");

            var entityLines = GenerateEntityConfiguration(entity);
            if (entityLines.Count > 0)
            {
                foreach (var line in entityLines)
                    sb.AppendLine($"        {line}");
                sb.AppendLine();
            }

            sb.AppendLine();

            ConfigureProperties(sb, entity);

            sb.AppendLine("    }");
            sb.AppendLine("}");
        }

        private List<string> GenerateEntityConfiguration(EntityModel entity)
        {
            return _rules.GetCallsForEntity(entity).ToList();
        }

        private void ConfigureProperties(StringBuilder sb, EntityModel entity)
        {
            var metadata = new EntityMetadata(entity, _entityNames);

            foreach (var prop in entity.Properties)
            {
                if (metadata.IsNavigationProperty(prop) || metadata.IsIgnored(prop.Name))
                {
                    continue;
                }

                var propertyLines = _rules.GetCallsForProperty(prop)
                                          .Where(line => !string.IsNullOrWhiteSpace(line))
                                          .ToList();

                if (ShouldSkipProperty(prop.Name, propertyLines.Count, metadata))
                {
                    continue;
                }

                sb.Append($"        builder.Property(x => x.{prop.Name})");

                foreach (var line in propertyLines.Distinct(StringComparer.Ordinal))
                {
                    sb.Append(line);
                }

                sb.AppendLine(";");
            }
        }

        private static bool ShouldSkipProperty(string propName, int lineCount, EntityMetadata metadata)
        {
            if (lineCount > 0)
                return false;

            return metadata.IsKey(propName) || metadata.IsForeignKey(propName);
        }

        private static StringBuilder AddHeader(string rootNs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// <auto-generated />");
            sb.AppendLine($"namespace {rootNs};");
            sb.AppendLine();
            sb.AppendLine("using Microsoft.EntityFrameworkCore;");
            sb.AppendLine("using Microsoft.EntityFrameworkCore.Metadata.Builders;");
            sb.AppendLine();
            return sb;
        }

    }
}
