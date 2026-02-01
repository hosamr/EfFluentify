using EFfluentify.Domain.Models;
using EFfluentify.Domain.Rules.EntityRules;
using EFfluentify.Domain.Rules.Helpers;
using EFfluentify.Domain.Rules.Interfaces;
using EFfluentify.Domain.Rules.PropertyRules;

namespace EFfluentify.Domain.Rules
{
    public sealed class RuleRegistry
    {
        private readonly List<IPropertyFluentRule> _propertyRules = new();
        private readonly List<IEntityFluentRule> _entityRules = new();


        public RuleRegistry Add(IPropertyFluentRule propertyRule, IEntityFluentRule entityRule)
        {
            _propertyRules.Add(propertyRule);
            _entityRules.Add(entityRule);
            return this;
        }

        public IEnumerable<string> GetCallsForProperty(PropertyModel property)
        {
            if (property.IsNullable)
                property.Attributes.Add(new AttributeModel { Name = "Nullable" });

            foreach (var attr in property.Attributes)
            {
                foreach (var rule in _propertyRules)
                {
                    if (rule.CanApply(attr, property))
                    {
                        var call = rule.GetFluentCall(attr, property);
                        if (!string.IsNullOrWhiteSpace(call))
                            yield return call!;
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

        public static RuleRegistry Default(IReadOnlyCollection<EntityModel> allEntities = default!)
        {
            var ctx = new ModelContext(allEntities);
            var registry = new RuleRegistry();

            registry._propertyRules.AddRange(new IPropertyFluentRule[]
            {
                new RequiredToIsRequiredRule(),
                new MaxLengthToHasMaxLengthRule(),
                new MinLengthToHasMinLengthRule(),
                new ColumnToHasColumnRule(),
                new DatabaseGeneratedToValueGeneratedRule(),
                new ConcurrencyCheckToIsConcurrencyTokenRule(),
                new TimestampToRowVersionRule(),
                new PrecisionFluentRule(),
                new UnicodeFluentRule(),
                new NullablePropertyFluentRule()
            });
            registry._entityRules.AddRange(new IEntityFluentRule[]
            {
                new TableAttributeToToTableRule(),
                new KeylessAttributeToHasNoKeyRule(),
                new CommentAttributeToHasCommentRule(),
                new IndexAttributeToHasIndexRule(),
                new KeyAttributeToHasKeyRule(),
                new NotMappedEntityFluentRule(),
                new ForeignKeyFluentRule(ctx)
            });

            return registry;
        }
    }
}
