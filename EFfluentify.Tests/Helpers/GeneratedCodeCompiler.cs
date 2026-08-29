using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EFfluentify.Tests.Helpers
{
    public static class GeneratedCodeCompiler
    {
        public sealed record CompileResult(bool Success, IReadOnlyList<string> Errors)
        {
            public string FormatErrors() =>
                Errors.Count == 0
                    ? "<none>"
                    : Environment.NewLine + string.Join(Environment.NewLine, Errors);
        }

        private static readonly string[] GlobalNamespaces =
        {
            "System",
            "System.Collections.Generic",
            "System.Linq",
            "System.ComponentModel.DataAnnotations",
            "System.ComponentModel.DataAnnotations.Schema",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.EntityFrameworkCore.Metadata.Builders",
        };

        public static CompileResult Compile(
            IEnumerable<string> entitySources,
            IEnumerable<string> generatedSources)
        {
            var parseOptions = new CSharpParseOptions(LanguageVersion.Latest);
            var trees = new List<SyntaxTree>();
            var namespaces = new HashSet<string>(GlobalNamespaces, StringComparer.Ordinal);

            foreach (var source in entitySources)
            {
                var tree = CSharpSyntaxTree.ParseText(source, parseOptions);
                trees.Add(tree);

                foreach (var nsNode in tree.GetRoot()
                                           .DescendantNodes()
                                           .OfType<BaseNamespaceDeclarationSyntax>())
                {
                    namespaces.Add(nsNode.Name.ToString());
                }
            }

            foreach (var source in generatedSources)
                trees.Add(CSharpSyntaxTree.ParseText(source, parseOptions));

            var globalUsings = string.Join(
                Environment.NewLine,
                namespaces.Select(ns => $"global using global::{ns};"));
            trees.Add(CSharpSyntaxTree.ParseText(globalUsings, parseOptions));

            var compilation = CSharpCompilation.Create(
                assemblyName: "EFfluentify.CompileCheck_" + Guid.NewGuid().ToString("N"),
                syntaxTrees: trees,
                references: BuildReferences(),
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    nullableContextOptions: NullableContextOptions.Enable));

            var errors = compilation
                .GetDiagnostics()
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .OrderBy(d => d.Location.SourceSpan.Start)
                .Select(FormatDiagnostic)
                .ToList();

            return new CompileResult(errors.Count == 0, errors);
        }

        private static string FormatDiagnostic(Diagnostic d)
        {
            var line = d.Location.IsInSource
                ? $" (line {d.Location.GetLineSpan().StartLinePosition.Line + 1})"
                : "";
            return $"{d.Id}: {d.GetMessage()}{line}";
        }

        private static IReadOnlyList<MetadataReference> BuildReferences()
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string tpa)
            {
                foreach (var path in tpa.Split(Path.PathSeparator))
                    if (!string.IsNullOrEmpty(path))
                        paths.Add(path);
            }

            void Add(Type t)
            {
                if (!string.IsNullOrEmpty(t.Assembly.Location))
                    paths.Add(t.Assembly.Location);
            }

            Add(typeof(object));
            Add(typeof(Enumerable));
            Add(typeof(System.Linq.Expressions.Expression));
            Add(typeof(System.Collections.Generic.ICollection<>));
            Add(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute));
            Add(typeof(System.ComponentModel.DataAnnotations.Schema.TableAttribute));
            Add(typeof(Microsoft.EntityFrameworkCore.IEntityTypeConfiguration<>));
            Add(typeof(Microsoft.EntityFrameworkCore.DbContext));
            Add(typeof(Microsoft.EntityFrameworkCore.RelationalPropertyBuilderExtensions));

            return paths
                .Where(File.Exists)
                .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                .ToList();
        }
    }
}
