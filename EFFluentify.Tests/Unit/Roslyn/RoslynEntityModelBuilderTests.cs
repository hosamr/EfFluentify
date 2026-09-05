using EFFluentify.Domain.Models;
using EFFluentify.Infrastructure.Roslyn;
using EFFluentify.Tests.Helpers;
using Xunit;

namespace EFFluentify.Tests.Unit.Roslyn
{
    [Trait("Category", "Unit")]
    public class RoslynEntityModelBuilderTests
    {
        private static async Task<List<EntityModel>> Build(params (string Path, string Content)[] files)
        {
            var builder = new RoslynEntityModelBuilder(new InMemoryFileManager(files));
            return await builder.BuildFromInputsAsync(files.Select(f => f.Path));
        }

        [Fact]
        public async Task ParsesClassName_Namespace_AndProperties()
        {
            var entities = await Build(("Foo.cs", @"
namespace My.Space
{
    public class Foo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}"));

            var foo = Assert.Single(entities);
            Assert.Equal("Foo", foo.Name);
            Assert.Equal("My.Space", foo.Namespace);
            Assert.Collection(foo.Properties,
                p => { Assert.Equal("Id", p.Name); Assert.Equal("int", p.TypeName); Assert.False(p.IsNullable); },
                p => { Assert.Equal("Name", p.Name); Assert.Equal("string?", p.TypeName); Assert.True(p.IsNullable); });
        }

        [Fact]
        public async Task ParsesFileScopedNamespace()
        {
            var entities = await Build(("Foo.cs", "namespace A.B;\npublic class Foo { public int Id { get; set; } }"));
            Assert.Equal("A.B", Assert.Single(entities).Namespace);
        }

        [Fact]
        public async Task NoNamespace_YieldsEmptyString()
        {
            var entities = await Build(("Foo.cs", "public class Foo { public int Id { get; set; } }"));
            Assert.Equal("", Assert.Single(entities).Namespace);
        }

        [Fact]
        public async Task ParsesMultipleClassesInOneFile()
        {
            var entities = await Build(("Models.cs", @"
namespace N
{
    public class A { public int Id { get; set; } }
    public class B { public int Id { get; set; } }
}"));

            Assert.Equal(new[] { "A", "B" }, entities.Select(e => e.Name).ToArray());
        }

        [Fact]
        public async Task ParsesPositionalAndNamedAttributeArguments()
        {
            var entities = await Build(("User.cs", @"
using System.ComponentModel.DataAnnotations.Schema;
namespace N
{
    [Table(""Users"", Schema = ""dbo"")]
    public class User
    {
        [MaxLength(50)]
        public string Name { get; set; }
    }
}"));

            var user = Assert.Single(entities);

            var table = Assert.Single(user.Attributes);
            Assert.Equal("Table", table.Name);
            Assert.Equal("\"Users\"", Assert.Single(table.PositionalArgs));
            Assert.Equal("\"dbo\"", table.NamedArgs["Schema"]);

            var name = Assert.Single(user.Properties);
            var maxLen = Assert.Single(name.Attributes);
            Assert.Equal("MaxLength", maxLen.Name);
            Assert.Equal("50", Assert.Single(maxLen.PositionalArgs));
        }

        [Fact]
        public async Task PreservesNameofExpressionInAttributeArgument()
        {
            var entities = await Build(("User.cs", @"
namespace N
{
    public class User
    {
        public int ManagerId { get; set; }
        [ForeignKey(nameof(ManagerId))]
        public User Manager { get; set; }
    }
}"));

            var manager = entities.Single().Properties.Single(p => p.Name == "Manager");
            var fk = Assert.Single(manager.Attributes);
            Assert.Equal("ForeignKey", fk.Name);
            Assert.Equal("nameof(ManagerId)", Assert.Single(fk.PositionalArgs));
        }

        [Fact]
        public async Task NestedClass_IsNotEmittedAsSeparateEntity()
        {
            var entities = await Build(("Outer.cs", @"
namespace N
{
    public class Outer
    {
        public int Id { get; set; }
        public class Inner { public string X { get; set; } }
    }
}"));

            var outer = Assert.Single(entities);
            Assert.Equal("Outer", outer.Name);
        }

        [Fact]
        public async Task NestedClass_PropertiesDoNotLeakIntoParent()
        {
            var entities = await Build(("Outer.cs", @"
namespace N
{
    public class Outer
    {
        public int Id { get; set; }
        public class Inner { public string X { get; set; } }
    }
}"));

            var outer = Assert.Single(entities);
            Assert.Equal(new[] { "Id" }, outer.Properties.Select(p => p.Name).ToArray());
        }

        [Fact]
        public async Task IgnoresNonCSharpFiles()
        {
            var entities = await Build(
                ("notes.txt", "this is not code { class X }"),
                ("Foo.cs", "namespace N; public class Foo { public int Id { get; set; } }"));

            Assert.Equal("Foo", Assert.Single(entities).Name);
        }

        [Fact]
        public async Task EmptyFile_ProducesNoEntities()
        {
            var entities = await Build(("Empty.cs", "using System;"));
            Assert.Empty(entities);
        }

        [Fact]
        public void Constructor_NullFileManager_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new RoslynEntityModelBuilder(null!));
        }
    }
}
