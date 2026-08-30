using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.EntityRules;
using EFFluentify.Domain.Rules.Helpers;
using EFFluentify.Domain.Rules.Interfaces;
using EFFluentify.Domain.Rules.PropertyRules;

namespace EFFluentify.Domain.Rules
{
    public class RuleRegistryFactory : IRuleRegistryFactory
    {
        public IRuleRegistry Create(IEnumerable<EntityModel>? allEntities = null)
        {
            var entities = allEntities?.ToList() ?? new List<EntityModel>();
            var ctx = new ModelContext(entities);

            var propertyRules = new IPropertyFluentRule[]
            {
                new RequiredToIsRequiredRule(),
                new MaxLengthToHasMaxLengthRule(),
                new ColumnToHasColumnRule(),
                new DatabaseGeneratedToValueGeneratedRule(),
                new ConcurrencyCheckToIsConcurrencyTokenRule(),
                new TimestampToRowVersionRule(),
                new PrecisionFluentRule(),
                new UnicodeFluentRule(),
                new DefaultValueToHasDefaultValueRule(),
                new CommentToHasCommentRule(),
                new NullablePropertyFluentRule()
            };

            var entityRules = new IEntityFluentRule[]
            {
                new TableAttributeToToTableRule(),
                new KeylessAttributeToHasNoKeyRule(),
                new CommentAttributeToHasCommentRule(),
                new IndexAttributeToHasIndexRule(),
                new KeyAttributeToHasKeyRule(),
                new NotMappedEntityFluentRule(),
                new ForeignKeyFluentRule(ctx)
            };

            return new RuleRegistry(propertyRules, entityRules);
        }
    }
}
