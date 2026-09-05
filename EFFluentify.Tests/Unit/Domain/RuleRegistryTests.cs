using EFFluentify.Domain.Rules;
using EFFluentify.Tests.Helpers;
using Xunit;
using static EFFluentify.Tests.Helpers.ModelFactory;

namespace EFFluentify.Tests.Unit.Domain
{
    [Trait("Category", "Unit")]
    public class RuleRegistryTests
    {
        private static readonly RuleRegistryFactory Factory = new();

        [Fact]
        public void GetCallsForProperty_InjectsSyntheticNullable_ForNullableNonKey()
        {
            var registry = Factory.Create();
            var prop = Prop("Email", "string?", nullable: true);

            Assert.Equal(new[] { ".IsRequired(false)" }, registry.GetCallsForProperty(prop).ToList());
        }

        [Fact]
        public void GetCallsForProperty_NoSyntheticNullable_ForNullableIdProperty()
        {
            var registry = Factory.Create();
            var prop = Prop("ManagerId", "int?", nullable: true);

            Assert.Empty(registry.GetCallsForProperty(prop));
        }

        [Fact]
        public void GetCallsForProperty_CombinesMultipleAttributes_InOrder()
        {
            var registry = Factory.Create();
            var prop = Prop("Name", "string", nullable: false,
                Attr("Required"), Attr("MaxLength", "50"));

            Assert.Equal(new[] { ".IsRequired()", ".HasMaxLength(50)" }, registry.GetCallsForProperty(prop).ToList());
        }

        [Fact]
        public void GetCallsForEntity_CombinesEntityRules()
        {
            var registry = Factory.Create();
            var entity = Entity("Widget",
                props: new[] { Prop("Id", "int", attrs: new[] { Attr("Key") }) },
                attrs: new[] { Attr("Table", "\"Widgets\"") });

            Assert.Equal(
                new[] { "builder.ToTable(\"Widgets\");", "builder.HasKey(e => e.Id);" },
                registry.GetCallsForEntity(entity).ToList());
        }

        [Fact]
        public void AllAnnotationAttributeNames_IncludesPropertyAndEntityAnnotations()
        {
            var names = Factory.Create().AllAnnotationAttributeNames().ToHashSet(StringComparer.Ordinal);

            // property-rule annotations
            Assert.Contains("Required", names);
            Assert.Contains("MaxLength", names);
            Assert.Contains("Nullable", names);
            // entity-rule annotations expand to both bare and *Attribute forms
            Assert.Contains("Table", names);
            Assert.Contains("TableAttribute", names);
            Assert.Contains("ForeignKey", names);
            Assert.Contains("ForeignKeyAttribute", names);
            Assert.Contains("Key", names);
            Assert.Contains("NotMapped", names);
        }

        [Fact]
        public void Registry_WithNullRuleLists_DoesNotThrow_AndReturnsEmpty()
        {
            var registry = new RuleRegistry(null!, null!);

            Assert.Empty(registry.GetCallsForProperty(Prop("Email", "string?", nullable: true)));
            Assert.Empty(registry.GetCallsForEntity(Entity("X")));
            Assert.Empty(registry.AllAnnotationAttributeNames());
        }
    }
}
