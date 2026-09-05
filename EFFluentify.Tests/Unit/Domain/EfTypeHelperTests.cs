using EFFluentify.Domain.Helpers;
using EFFluentify.Tests.Helpers;
using Xunit;

namespace EFFluentify.Tests.Unit.Domain
{
    [Trait("Category", "Unit")]
    public class EfTypeHelperTests
    {
        [Theory]
        [InlineData("List<Post>", true)]
        [InlineData("ICollection<Post>", true)]
        [InlineData("IEnumerable<Post>", true)]
        [InlineData("HashSet<Post>", true)]
        [InlineData("System.Collections.Generic.List<Post>", true)]
        [InlineData("int[]", true)]
        [InlineData("Post", false)]
        [InlineData("int", false)]
        [InlineData("Dictionary<int, string>", false)]
        [InlineData("", false)]
        [InlineData("   ", false)]
        public void IsCollectionType_Classifies_Correctly(string type, bool expected)
        {
            Assert.Equal(expected, EfTypeHelper.IsCollectionType(type));
        }

        [Fact]
        public void IsCollectionType_Null_ReturnsFalse()
        {
            Assert.False(EfTypeHelper.IsCollectionType(null!));
        }

        [Theory]
        [InlineData("List<Post>", "Post")]
        [InlineData("ICollection<Blog?>", "Blog")]
        [InlineData("System.Collections.Generic.List<Order>", "Order")]
        [InlineData("int", null)]
        [InlineData("Post", null)]
        public void TryGetCollectionElementType_ExtractsGenericArgument(string type, string? expected)
        {
            Assert.Equal(expected, EfTypeHelper.TryGetCollectionElementType(type));
        }

        [Theory]
        [InlineData("int", true)]
        [InlineData("int?", true)]
        [InlineData("string", true)]
        [InlineData("decimal", true)]
        [InlineData("Guid", true)]
        [InlineData("DateTime", true)]
        [InlineData("DateTimeOffset", true)]
        [InlineData("byte[]", true)]
        [InlineData("Blog", false)]
        [InlineData("Int32", false)]
        [InlineData("List<int>", false)]
        public void IsScalarType_Classifies_Correctly(string type, bool expected)
        {
            Assert.Equal(expected, EfTypeHelper.IsScalarType(type));
        }

        [Theory]
        [InlineData("Blog?", "Blog")]
        [InlineData("  Foo.Bar.Baz  ", "Baz")]
        [InlineData("System.Collections.Generic.List<Blog>", "List<Blog>")]
        [InlineData("int", "int")]
        public void NormalizeTypeName_StripsNamespaceAndNullable(string input, string expected)
        {
            Assert.Equal(expected, EfTypeHelper.NormalizeTypeName(input));
        }

        [Theory]
        [InlineData("nameof(ManagerId)", "ManagerId")]
        [InlineData("\"BlogId\"", "BlogId")]
        [InlineData("nameof(Order.Id)", "Order.Id")]
        [InlineData("   ", null)]
        [InlineData("\"\"", null)]
        public void NormalizeMemberName_UnwrapsNameofAndQuotes(string input, string? expected)
        {
            Assert.Equal(expected, EfTypeHelper.NormalizeMemberName(input));
        }

        [Fact]
        public void SplitFkNames_SplitsCompositeKeys()
        {
            var names = EfTypeHelper.SplitFkNames("\"MyTeamId, MyLeagueId\"").ToList();
            Assert.Equal(new[] { "MyTeamId", "MyLeagueId" }, names);
        }

        [Fact]
        public void SplitFkNames_SingleName_ReturnsOne()
        {
            Assert.Equal(new[] { "ManagerId" }, EfTypeHelper.SplitFkNames("nameof(ManagerId)").ToList());
        }

        [Fact]
        public void SplitFkNames_Blank_ReturnsEmpty()
        {
            Assert.Empty(EfTypeHelper.SplitFkNames("   "));
        }

        [Theory]
        [InlineData("Blog", true)]          // direct reference to a known entity
        [InlineData("ICollection<Post>", true)] // collection of known entity
        [InlineData("List<Post>", true)]
        [InlineData("Post[]", true)]        // array of known entity
        [InlineData("int", false)]
        [InlineData("string", false)]
        [InlineData("Unknown", false)]
        [InlineData("", false)]
        [InlineData("Dictionary<string, Blog>", false)] // two type args: not a known collection type
        [InlineData("<Blog>", false)]                    // malformed generic: no outer type
        public void IsNavigationProperty_DetectsEntityReferences(string type, bool expected)
        {
            var entityNames = new HashSet<string>(StringComparer.Ordinal) { "Blog", "Post" };
            var prop = ModelFactory.Prop("Nav", type);

            Assert.Equal(expected, EfTypeHelper.IsNavigationProperty(prop, entityNames));
        }

        [Fact]
        public void HasOneToOneSignal_True_WhenPropertyHasKeyAndForeignKey()
        {
            var entity = ModelFactory.Entity("UserProfile", new[]
            {
                ModelFactory.Prop("Id", "int", attrs: new[]
                {
                    ModelFactory.Attr("Key"),
                    ModelFactory.Attr("ForeignKey", "\"User\"")
                })
            });

            Assert.True(EfTypeHelper.HasOneToOneSignal(entity));
        }

        [Fact]
        public void HasOneToOneSignal_False_WhenKeyAndForeignKeyOnDifferentProperties()
        {
            var entity = ModelFactory.Entity("Thing", new[]
            {
                ModelFactory.Prop("Id", "int", attrs: new[] { ModelFactory.Attr("Key") }),
                ModelFactory.Prop("OtherId", "int", attrs: new[] { ModelFactory.Attr("ForeignKey", "\"Other\"") })
            });

            Assert.False(EfTypeHelper.HasOneToOneSignal(entity));
        }

        [Theory]
        [InlineData("Key", true)]
        [InlineData("KeyAttribute", true)]
        [InlineData("Required", false)]
        public void IsKeyAttribute_MatchesBothForms(string name, bool expected)
        {
            Assert.Equal(expected, EfTypeHelper.IsKeyAttribute(ModelFactory.Attr(name)));
        }

        [Theory]
        [InlineData("ForeignKey", true)]
        [InlineData("ForeignKeyAttribute", true)]
        [InlineData("Key", false)]
        public void IsForeignKeyAttribute_MatchesBothForms(string name, bool expected)
        {
            Assert.Equal(expected, EfTypeHelper.IsForeignKeyAttribute(ModelFactory.Attr(name)));
        }
    }
}
