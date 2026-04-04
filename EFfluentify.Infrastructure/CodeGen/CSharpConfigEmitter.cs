using System.Text;
using EFfluentify.Application.Helpers;
using EFfluentify.Application.Interfaces;
using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules;
using EFfluentify.Application.Models;

namespace EFfluentify.Infrastructure.CodeGen
{
    public sealed class CSharpConfigEmitter : ICodeGenerator
    {
        private RuleRegistry _rules;
        private IReadOnlyList<EntityModel> _entities = [];
        public CSharpConfigEmitter()
        {
            _entities = [];
            _rules = default!;
        }
        public Dictionary<string, string> Generate(IEnumerable<EntityModel> entities, PipelineOptions options, RuleRegistry rules)
        {
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _entities = entities?.ToList() ?? throw new ArgumentNullException(nameof(entities));

            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (options.ManyFiles)
            {
                foreach (var e in _entities)
                {
                    var code = GenerateSingle(options.RootNamespace, e);
                    var fileName = $"{e.Name}Configuration.cs";
                    map[fileName] = code;
                }
            }
            else
            {
                var sb = AddHeader(options.RootNamespace);
                foreach (var e in _entities)
                {
                    AppendConfig(sb, e);
                    sb.AppendLine();
                }
                map["EntityConfigurations.cs"] = sb.ToString();
            }
            return map;
        }

        private string GenerateSingle(string rootNs, EntityModel e)
        {
            var sb = AddHeader(rootNs);
            AppendConfig(sb, e);
            return sb.ToString();
        }

        private void AppendConfig(StringBuilder sb, EntityModel entity)
        {
            sb.AppendLine($"internal sealed class {entity.Name}Configuration : IEntityTypeConfiguration<{entity.Name}>");
            sb.AppendLine("{");
            sb.AppendLine($"    public void Configure(EntityTypeBuilder<{entity.Name}> builder)");
            sb.AppendLine("    {");

            var configLines = GenerateEntityConfiguration(entity);
            foreach (var line in configLines)
            {
                sb.AppendLine($"        {line}");
            }

            sb.AppendLine();

            var keyProps = GetKeyProperties(entity);
            var ignoredProps = GetIgnoredProperties(entity);
            var fkProps = GetForeignKeyProperties(entity);

            ConfigureProperties(sb, entity, keyProps, ignoredProps, fkProps);

            sb.AppendLine("    }");
            sb.AppendLine("}");
        }

        private List<string> GenerateEntityConfiguration(EntityModel entity)
        {
            var entityLines = _rules.GetCallsForEntity(entity).ToList();
            if (entityLines.Count > 0)
            {
                return entityLines;
            }
            return [$"builder.ToTable(\"{entity.Name}\");"];
        }

        private static HashSet<string> GetKeyProperties(EntityModel entity)
        {
            return entity.Properties
                .Where(p => p.Attributes.Any(a => a.Name is "Key" or "KeyAttribute"))
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static HashSet<string> GetIgnoredProperties(EntityModel entity)
        {
            return entity.Properties
                .Where(p => p.Attributes.Any(a => a.Name is "NotMapped" or "NotMappedAttribute"))
                .Select(p => p.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private HashSet<string> GetForeignKeyProperties(EntityModel entity)
        {
            var fkPropNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in entity.Properties)
            {
                var fkAttr = prop.Attributes.FirstOrDefault(a => a.Name is "ForeignKey" or "ForeignKeyAttribute");
                if (fkAttr == null)
                    continue;
                var arg0 = fkAttr.PositionalArgs.FirstOrDefault() as string;
                if (string.IsNullOrWhiteSpace(arg0))
                    continue;

                if (EfTypeClassifier.IsNavigationProperty(prop, _entities) || EfTypeClassifier.IsCollectionType(prop.TypeName))
                {
                    foreach (var fk in EfTypeClassifier.SplitFkNames(arg0))
                        fkPropNames.Add(fk);
                }
                else
                {
                    fkPropNames.Add(prop.Name);
                }
            }
            return fkPropNames;
        }

        private void ConfigureProperties(StringBuilder sb, EntityModel entity, HashSet<string> keyProps, HashSet<string> ignoredProps, HashSet<string> fkProps)
        {
            foreach (var prop in entity.Properties)
            {
                if (EfTypeClassifier.IsNavigationProperty(prop, _entities))
                    continue;

                if (ignoredProps.Contains(prop.Name))
                    continue;

                var calls = _rules.GetCallsForProperty(prop)
                            .Where(c => !string.IsNullOrWhiteSpace(c))
                            .ToList();

                if (keyProps.Contains(prop.Name) && calls.Count == 0)
                    continue;

                if (fkProps.Contains(prop.Name) && calls.Count == 0)
                    continue;

                sb.Append($"        builder.Property(x => x.{prop.Name})");

                foreach (var c in calls.Distinct(StringComparer.Ordinal))
                    sb.Append(c);

                sb.AppendLine(";");
            }
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
