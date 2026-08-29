using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.Interfaces;

namespace EFfluentify.Domain.Rules
{
    public sealed class RuleRegistry : IRuleRegistry
    {
        private readonly List<IPropertyFluentRule> _propertyRules;
        private readonly List<IEntityFluentRule> _entityRules;

        public RuleRegistry(IEnumerable<IPropertyFluentRule> propertyRules, IEnumerable<IEntityFluentRule> entityRules)
        {
            _propertyRules = propertyRules?.ToList() ?? new List<IPropertyFluentRule>();
            _entityRules = entityRules?.ToList() ?? new List<IEntityFluentRule>();
        }

        public IEnumerable<string> GetCallsForProperty(Property property)
        {
            var attributes = property.Attributes.ToList();
            if (property.IsNullable && !attributes.Any(a => a.Name == "Nullable"))
                attributes.Add(new AttributeEntry { Name = "Nullable" });

            foreach (var attr in attributes)
            {
                foreach (var rule in _propertyRules)
                {
                    if (rule.CanApply(attr, property))
                    {
                        var lines = rule.GetFluentLines(attr, property);
                        foreach (var line in lines)
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                                yield return line;
                        }
                    }
                }
            }

        }

        public IEnumerable<string> GetCallsForEntity(EntityModel e)
        {
            var entityLines = _entityRules.Where(r => r.CanApply(e)).SelectMany(r => r.GetFluentLines(e)).ToList();
            return entityLines;
        }
        public IEnumerable<string> AllAnnotationAttributeNames()
        {
            foreach (var r in _propertyRules)
            {
                foreach (var n in r.GetAnnotationPropertyNames())
                    yield return n;
            }
            foreach (var r in _entityRules)
            {
                foreach (var n in r.GetAnnotationAttributeNames())
                    yield return n;
            }
        }

    }
}
