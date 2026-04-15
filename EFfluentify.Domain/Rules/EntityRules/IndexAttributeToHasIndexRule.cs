using EFfluentify.Application.Helpers;
using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules.EntityRules
{
    public sealed class IndexAttributeToHasIndexRule : IEntityFluentRule
    {
        public bool CanApply(EntityModel entity)
            => entity.Attributes.Any(a => a.Name is "Index" or "IndexAttribute");

        public IEnumerable<string> GetFluentLines(EntityModel entity)
        {
            var indexAttrs = entity.Attributes
                .Where(a => a.Name is "Index" or "IndexAttribute");

            foreach (var indexAttr in indexAttrs)
            {
                var propertyNames = ExtractPropertyNames(indexAttr).ToList();
                if (propertyNames.Count == 0)
                    continue;

                indexAttr.NamedArgs.TryGetValue("Name", out var nameObj);
                indexAttr.NamedArgs.TryGetValue("IsUnique", out var isUniqueObj);

                var indexName = nameObj as string;
                bool.TryParse(isUniqueObj, out bool isUnique);

                var lambda = BuildIndexLambda(propertyNames);

                var line = $"builder.HasIndex({lambda})";

                if (isUnique == true)
                    line += ".IsUnique()";

                if (!string.IsNullOrWhiteSpace(indexName))
                    line += $".HasDatabaseName({indexName})";

                line += ";";
                yield return line;
            }
        }

        public IEnumerable<string> GetAnnotationAttributeNames()
        {
            yield return "Index";
            yield return "IndexAttribute";
        }

        private static IReadOnlyList<string> ExtractPropertyNames(AttributeEntry indexAttr)
        {
            var results = new List<string>();

            foreach (var raw in indexAttr.PositionalArgs)
            {
                foreach (var name in SplitIfCombined(raw))
                {
                    var normalized = EfTypeClassifier.NormalizeMemberName(name);
                    if (!string.IsNullOrWhiteSpace(normalized))
                        results.Add(normalized);
                }
            }

            return results;
        }

        private static IEnumerable<string> SplitIfCombined(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                yield break;

            var parts = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
                yield return p.Trim();
        }
        
        private static string BuildIndexLambda(IReadOnlyList<string> props)
        {
            if (props.Count == 1)
                return $"e => e.{props[0]}";

            var fields = string.Join(", ", props.Select(p => $"e.{p}"));
            return $"e => new {{ {fields} }}";
        }
    }

}
