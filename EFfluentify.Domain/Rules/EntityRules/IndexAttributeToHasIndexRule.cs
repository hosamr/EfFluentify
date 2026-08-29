using EFfluentify.Domain.Helpers;
using EFfluentify.Domain.Models;
using System.Text;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class IndexAttributeToHasIndexRule : EntityFluentRuleBase
    {
        protected override IEnumerable<string> SupportedAttributeNames => new[] { "Index" };

        protected override AttributeScope Scope => AttributeScope.Entity;

        public override IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            foreach (var indexAttr in entity.Attributes.Where(IsSupportedAttribute))
            {
                var propertyNames = ExtractPropertyNames(indexAttr);
                if (propertyNames.Count == 0)
                    continue;

                indexAttr.NamedArgs.TryGetValue("Name", out var indexName);
                indexAttr.NamedArgs.TryGetValue("IsUnique", out var isUniqueRaw);
                bool.TryParse(isUniqueRaw, out bool isUnique);

                yield return BuildStatement(propertyNames, indexName, isUnique);
            }
        }

        private static IReadOnlyList<string> ExtractPropertyNames(AttributeEntry indexAttr) {
            return indexAttr.PositionalArgs
                .SelectMany(raw => raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Select(EfTypeHelper.NormalizeMemberName)
                .OfType<string>()
                .ToList();
        }

        private static string BuildStatement(IReadOnlyList<string> props, string? indexName, bool isUnique)
        {
            var sb = new StringBuilder($"builder.HasIndex({BuildLambda(props)})");

            if (isUnique)
                sb.Append(".IsUnique()");

            if (!string.IsNullOrWhiteSpace(indexName))
                sb.Append($".HasDatabaseName({indexName})");

            return sb.Append(';').ToString();
        }

        private static string BuildLambda(IReadOnlyList<string> props) =>
            props.Count == 1
                ? $"e => e.{props[0]}"
                : $"e => new {{ {string.Join(", ", props.Select(p => $"e.{p}"))} }}";
    }
}
