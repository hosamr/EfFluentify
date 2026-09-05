using EFFluentify.Domain.Models;
using EFFluentify.Domain.Rules.Helpers;
using Xunit;
using static EFFluentify.Tests.Helpers.ModelFactory;

namespace EFFluentify.Tests.Unit.Domain
{
    [Trait("Category", "Unit")]
    public class EntityMetadataTests
    {
        private static EntityMetadata Build(EntityModel entity, params string[] entityNames)
        {
            return new EntityMetadata(entity, new HashSet<string>(entityNames, StringComparer.Ordinal));
        }

        [Fact]
        public void IsKey_TrueForKeyDecoratedProperty()
        {
            var entity = Entity("E", new[]
            {
                Prop("Code", "int", attrs: new[] { Attr("Key") }),
                Prop("Other", "int")
            });
            var meta = Build(entity, "E");

            Assert.True(meta.IsKey("Code"));
            Assert.False(meta.IsKey("Other"));
        }

        [Fact]
        public void IsIgnored_TrueForNotMappedProperty()
        {
            var entity = Entity("E", new[] { Prop("Temp", "string", attrs: new[] { Attr("NotMapped") }) });
            var meta = Build(entity, "E");

            Assert.True(meta.IsIgnored("Temp"));
        }

        [Fact]
        public void IsForeignKey_TrueForFkNameReferencedByNavigation()
        {
            var entity = Entity("Post", new[]
            {
                Prop("BlogId", "int"),
                Prop("Blog", "Blog", attrs: new[] { Attr("ForeignKey", "\"BlogId\"") })
            });
            var meta = Build(entity, "Post", "Blog");

            Assert.True(meta.IsForeignKey("BlogId"));
        }

        [Fact]
        public void IsForeignKey_TrueForScalarPropertyItself()
        {
            var entity = Entity("Book", new[]
            {
                Prop("AuthorId", "int", attrs: new[] { Attr("ForeignKey", "\"AuthorNav\"") }),
                Prop("AuthorNav", "Author")
            });
            var meta = Build(entity, "Book", "Author");

            Assert.True(meta.IsForeignKey("AuthorId"));
        }

        [Fact]
        public void IsNavigationProperty_DelegatesToTypeHelper()
        {
            var entity = Entity("Post", new[] { Prop("Blog", "Blog"), Prop("Id", "int") });
            var meta = Build(entity, "Post", "Blog");

            Assert.True(meta.IsNavigationProperty(entity.Properties[0]));
            Assert.False(meta.IsNavigationProperty(entity.Properties[1]));
        }

        [Fact]
        public void Constructor_WithEntityList_ResolvesForeignKeys()
        {
            var entity = Entity("Post", new[]
            {
                Prop("BlogId", "int"),
                Prop("Blog", "Blog", attrs: new[] { Attr("ForeignKey", "\"BlogId\"") })
            });

            var meta = new EntityMetadata(entity, new List<EntityModel> { entity, Entity("Blog") });

            Assert.True(meta.IsForeignKey("BlogId"));
        }

        [Fact]
        public void ForeignKey_WithBlankArgument_IsIgnored()
        {
            var entity = Entity("Post", new[] { Prop("Blog", "Blog", attrs: new[] { Attr("ForeignKey") }) });
            var meta = Build(entity, "Post", "Blog");

            Assert.False(meta.IsForeignKey("Blog"));
        }
    }

    [Trait("Category", "Unit")]
    public class ModelContextTests
    {
        [Fact]
        public void TryGetEntity_FindsByExactName()
        {
            var ctx = new ModelContext(new[] { Entity("Blog"), Entity("Post") });

            Assert.True(ctx.TryGetEntity("Blog", out var e));
            Assert.Equal("Blog", e.Name);
        }

        [Theory]
        [InlineData("My.Namespace.Blog")]
        [InlineData("Blog?")]
        [InlineData("  Blog  ")]
        public void TryGetEntity_NormalizesLookupName(string lookup)
        {
            var ctx = new ModelContext(new[] { Entity("Blog") });
            Assert.True(ctx.TryGetEntity(lookup, out _));
        }

        [Fact]
        public void TryGetEntity_ReturnsFalseForUnknown()
        {
            var ctx = new ModelContext(new[] { Entity("Blog") });
            Assert.False(ctx.TryGetEntity("Ghost", out _));
        }

        [Fact]
        public void Constructor_IgnoresBlankNamesAndKeepsFirstOnDuplicate()
        {
            var first = Entity("Dup", new[] { Prop("A", "int") });
            var second = Entity("Dup", new[] { Prop("B", "int") });
            var blank = Entity("");

            var ctx = new ModelContext(new[] { first, second, blank });

            Assert.True(ctx.TryGetEntity("Dup", out var e));
            Assert.Same(first, e);
            Assert.False(ctx.TryGetEntity("", out _));
        }

        [Fact]
        public void Constructor_WithNullEntities_IsEmpty()
        {
            var ctx = new ModelContext(null!);
            Assert.False(ctx.TryGetEntity("Anything", out _));
        }
    }
}
