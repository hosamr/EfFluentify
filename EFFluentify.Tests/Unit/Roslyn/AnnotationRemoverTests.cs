using EFFluentify.Application.Models;
using EFFluentify.Domain.Rules;
using EFFluentify.Infrastructure.Roslyn;
using EFFluentify.Tests.Helpers;
using Xunit;

namespace EFFluentify.Tests.Unit.Roslyn
{
    [Trait("Category", "Unit")]
    public class AnnotationRemoverTests
    {
        private static async Task<IReadOnlyList<AnnotationRemovalChange>> Prepare(params (string Path, string Content)[] files)
        {
            var remover = new AnnotationRemover(new InMemoryFileManager(files), new RuleRegistryFactory());
            return await remover.PrepareRemovalAsync(files.Select(f => f.Path));
        }

        [Fact]
        public async Task StripsTargetedAttribute_AndRecordsOriginal()
        {
            const string source = @"namespace N
{
    public class User
    {
        [Required]
        public string Name { get; set; }
    }
}";
            var changes = await Prepare(("User.cs", source));

            var change = Assert.Single(changes);
            Assert.Equal("User.cs", change.FilePath);
            Assert.Equal(source, change.OriginalContent);
            Assert.DoesNotContain("Required", change.UpdatedContent);
            Assert.Contains("Name", change.UpdatedContent);
        }

        [Fact]
        public async Task KeepsNonTargetedAttributes()
        {
            const string source = @"namespace N
{
    public class User
    {
        [JsonIgnore]
        [Required]
        public string Name { get; set; }
    }
}";
            var change = Assert.Single(await Prepare(("User.cs", source)));

            Assert.Contains("JsonIgnore", change.UpdatedContent);
            Assert.DoesNotContain("Required", change.UpdatedContent);
        }

        [Fact]
        public async Task MultipleFiles_AreOrderedByPath()
        {
            const string src = @"namespace N { public class C { [Required] public string X { get; set; } } }";
            var changes = await Prepare(("zeta.cs", src), ("alpha.cs", src));

            Assert.Equal(new[] { "alpha.cs", "zeta.cs" }, changes.Select(c => c.FilePath).ToArray());
        }

        [Fact]
        public async Task DoesNotStripNonTargetAttributes()
        {
            const string source = @"namespace N { public class C { [JsonIgnore] public int Id { get; set; } } }";

            // No mapped annotation is present, so nothing should be removed and the file must be
            // left completely untouched (not even reformatted).
            var changes = await Prepare(("C.cs", source));
            Assert.Empty(changes);
        }

        [Fact]
        public async Task FileWithNoAnnotationsAtAll_IsNotReportedAsChanged()
        {
            // Deliberately odd formatting: the remover must not report a whitespace-only rewrite.
            const string source = "namespace N {\r\n  public class Plain {\r\n     public int Id {get;set;}\r\n     public string  Name {get;set;}\r\n  }\r\n}\r\n";

            var changes = await Prepare(("Plain.cs", source));
            Assert.Empty(changes);
        }

        [Fact]
        public async Task StripsFullyQualifiedAttributeNames()
        {
            const string source = @"namespace N
{
    public class User
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; set; }
    }
}";
            var change = Assert.Single(await Prepare(("User.cs", source)));
            Assert.DoesNotContain("Required", change.UpdatedContent);
        }

        [Fact]
        public async Task StripsAliasQualifiedAttributeNames()
        {
            const string source = @"namespace N
{
    public class User
    {
        [global::Required]
        public string Name { get; set; }
    }
}";
            var change = Assert.Single(await Prepare(("User.cs", source)));
            Assert.DoesNotContain("Required", change.UpdatedContent);
        }

        [Fact]
        public void Constructor_NullArguments_Throw()
        {
            Assert.Throws<ArgumentNullException>(() => new AnnotationRemover(null!, new RuleRegistryFactory()));
            Assert.Throws<ArgumentNullException>(() => new AnnotationRemover(new InMemoryFileManager(), null!));
        }
    }
}
