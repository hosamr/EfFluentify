using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.EntityRules;
using EFFluentify.Domain.Rules.Interfaces;
using EFFluentify.Tests.Helpers;
using Xunit;

namespace EFFluentify.Tests.Unit.Domain
{
    [Trait("Category", "Unit")]
    public class EntityRulesTests
    {
        private static List<string> Apply(IEntityFluentRule rule, EntityModel entity) =>
            rule.CanApply(entity) ? rule.GetFluentLines(entity).ToList() : new List<string>();

        // ---- Table ----
        [Fact]
        public void Table_NameAndSchema()
        {
            var entity = ModelFactory.Entity("User", attrs: new[]
            {
                ModelFactory.Attr("Table", "\"Users\"").Named("Schema", "\"dbo\"")
            });
            Assert.Equal(new[] { "builder.ToTable(\"Users\", \"dbo\");" }, Apply(new TableAttributeToToTableRule(), entity));
        }

        [Fact]
        public void Table_NameOnly()
        {
            var entity = ModelFactory.Entity("User", attrs: new[] { ModelFactory.Attr("Table", "\"Users\"") });
            Assert.Equal(new[] { "builder.ToTable(\"Users\");" }, Apply(new TableAttributeToToTableRule(), entity));
        }

        [Fact]
        public void Table_NoName_FallsBackToEntityName()
        {
            var entity = ModelFactory.Entity("Order", attrs: new[] { ModelFactory.Attr("Table") });
            Assert.Equal(new[] { "builder.ToTable(\"Order\");" }, Apply(new TableAttributeToToTableRule(), entity));
        }

        [Fact]
        public void Table_MatchesAttributeSuffixForm()
        {
            var entity = ModelFactory.Entity("User", attrs: new[] { ModelFactory.Attr("TableAttribute", "\"Users\"") });
            Assert.True(new TableAttributeToToTableRule().CanApply(entity));
        }

        [Fact]
        public void Table_GetAnnotationAttributeNames_IncludesBothForms()
        {
            var names = new TableAttributeToToTableRule().GetAnnotationAttributeNames().ToList();
            Assert.Contains("Table", names);
            Assert.Contains("TableAttribute", names);
        }

        // ---- Key ----
        [Fact]
        public void Key_Single()
        {
            var entity = ModelFactory.Entity("User", new[]
            {
                ModelFactory.Prop("Id", "int", attrs: new[] { ModelFactory.Attr("Key") })
            });
            Assert.Equal(new[] { "builder.HasKey(e => e.Id);" }, Apply(new KeyAttributeToHasKeyRule(), entity));
        }

        [Fact]
        public void Key_Composite()
        {
            var entity = ModelFactory.Entity("Team", new[]
            {
                ModelFactory.Prop("TeamId", "int", attrs: new[] { ModelFactory.Attr("Key") }),
                ModelFactory.Prop("LeagueId", "int", attrs: new[] { ModelFactory.Attr("Key") })
            });
            Assert.Equal(new[] { "builder.HasKey(e => new { e.TeamId, e.LeagueId });" }, Apply(new KeyAttributeToHasKeyRule(), entity));
        }

        [Fact]
        public void Key_NoKeyProperties_DoesNotApply()
        {
            var entity = ModelFactory.Entity("User", new[] { ModelFactory.Prop("Id", "int") });
            Assert.False(new KeyAttributeToHasKeyRule().CanApply(entity));
        }

        // ---- Keyless ----
        [Fact]
        public void Keyless_Emits_HasNoKey()
        {
            var entity = ModelFactory.Entity("View", attrs: new[] { ModelFactory.Attr("Keyless") });
            Assert.Equal(new[] { "builder.HasNoKey();" }, Apply(new KeylessAttributeToHasNoKeyRule(), entity));
        }

        // ---- Index ----
        [Fact]
        public void Index_Unique_WithName()
        {
            var entity = ModelFactory.Entity("User", attrs: new[]
            {
                ModelFactory.Attr("Index", "nameof(Email)").Named("IsUnique", "true").Named("Name", "\"IX_User_Email\"")
            });
            Assert.Equal(
                new[] { "builder.HasIndex(e => e.Email).IsUnique().HasDatabaseName(\"IX_User_Email\");" },
                Apply(new IndexAttributeToHasIndexRule(), entity));
        }

        [Fact]
        public void Index_Simple()
        {
            var entity = ModelFactory.Entity("User", attrs: new[] { ModelFactory.Attr("Index", "nameof(Email)") });
            Assert.Equal(new[] { "builder.HasIndex(e => e.Email);" }, Apply(new IndexAttributeToHasIndexRule(), entity));
        }

        [Fact]
        public void Index_Composite()
        {
            var entity = ModelFactory.Entity("User", attrs: new[]
            {
                ModelFactory.Attr("Index", "nameof(First)", "nameof(Last)")
            });
            Assert.Equal(new[] { "builder.HasIndex(e => new { e.First, e.Last });" }, Apply(new IndexAttributeToHasIndexRule(), entity));
        }

        // ---- Comment (entity) ----
        [Fact]
        public void EntityComment_Emits_HasComment()
        {
            var entity = ModelFactory.Entity("User", attrs: new[] { ModelFactory.Attr("Comment", "\"A user\"") });
            Assert.Equal(new[] { "builder.HasComment(\"A user\");" }, Apply(new CommentAttributeToHasCommentRule(), entity));
        }

        // ---- NotMapped ----
        [Fact]
        public void NotMapped_Ignores_EachMarkedProperty()
        {
            var entity = ModelFactory.Entity("User", new[]
            {
                ModelFactory.Prop("Id", "int"),
                ModelFactory.Prop("Temp", "string", attrs: new[] { ModelFactory.Attr("NotMapped") }),
                ModelFactory.Prop("Temp2", "string", attrs: new[] { ModelFactory.Attr("NotMapped") })
            });
            Assert.Equal(
                new[] { "builder.Ignore(e => e.Temp);", "builder.Ignore(e => e.Temp2);" },
                Apply(new NotMappedEntityFluentRule(), entity));
        }

        // ---- Guard clauses (GetFluentLines called directly, bypassing CanApply) ----
        [Fact]
        public void Table_GetFluentLines_WithoutTableAttribute_YieldsNothing()
        {
            Assert.Empty(new TableAttributeToToTableRule().GetFluentLines(ModelFactory.Entity("Order")));
        }

        [Fact]
        public void Index_WithNoPropertyNames_YieldsNothing()
        {
            var entity = ModelFactory.Entity("User", attrs: new[] { ModelFactory.Attr("Index") });
            Assert.Empty(Apply(new IndexAttributeToHasIndexRule(), entity));
        }

        [Fact]
        public void EntityComment_WithoutComment_YieldsNothing()
        {
            Assert.Empty(new CommentAttributeToHasCommentRule().GetFluentLines(ModelFactory.Entity("User")));
        }
    }
}
