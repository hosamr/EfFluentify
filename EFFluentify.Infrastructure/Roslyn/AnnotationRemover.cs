using EFFluentify.Application.Interfaces;
using EFFluentify.Application.Models;
using EFFluentify.Domain.Rules.Interfaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EFFluentify.Infrastructure.Roslyn
{
    public class AnnotationRemover : IAnnotationRemover
    {
        private readonly IFileManager _fileManager;
        private readonly IRuleRegistryFactory _ruleRegistryFactory;

        public AnnotationRemover(IFileManager fileManager, IRuleRegistryFactory ruleRegistryFactory)
        {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
            _ruleRegistryFactory = ruleRegistryFactory ?? throw new ArgumentNullException(nameof(ruleRegistryFactory));
        }

        public async Task<IReadOnlyList<AnnotationRemovalChange>> PrepareRemovalAsync(IEnumerable<string> inputs)
        {
            var rules = _ruleRegistryFactory.Create();
            var files = _fileManager.ExpandFiles(inputs)
                         .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                         .ToList();

            var targets = rules.AllAnnotationAttributeNames()
                .Select(TrimAttributeSuffix)
                .ToHashSet(StringComparer.Ordinal);

            var results = new System.Collections.Concurrent.ConcurrentBag<AnnotationRemovalChange>();

            await Parallel.ForEachAsync(files, async (file, cancellationToken) =>
            {
                var original = await _fileManager.ReadFileAsync(file);
                var tree = CSharpSyntaxTree.ParseText(original, cancellationToken: cancellationToken);
                var root = tree.GetCompilationUnitRoot(cancellationToken);

                var rewriter = new AnnotationStripper(targets);
                var newRoot = (CompilationUnitSyntax)rewriter.Visit(root);

                var updated = newRoot.NormalizeWhitespace().ToFullString();

                if (!string.Equals(original, updated, StringComparison.Ordinal))
                {
                    results.Add(new AnnotationRemovalChange(file, original, updated));
                }
            });

            return results.OrderBy(r => r.FilePath).ToList();
        }

        private static string TrimAttributeSuffix(string name)
        {
            return name.EndsWith("Attribute", StringComparison.Ordinal) ? name[..^9] : name;
        }

        private sealed class AnnotationStripper : CSharpSyntaxRewriter
        {
            private readonly HashSet<string> _targets;

            public AnnotationStripper(HashSet<string> targets)
            {
                _targets = targets;
            }

            public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
            {
                var kept = new SeparatedSyntaxList<AttributeSyntax>();
                foreach (var attr in node.Attributes)
                {
                    if (!IsTarget(attr))
                        kept = kept.Add(attr);
                }

                if (kept.Count == 0) return null;

                return node.WithAttributes(kept);
            }

            private bool IsTarget(AttributeSyntax attr)
            {
                var baseName = GetBaseAttributeName(attr.Name);
                return _targets.Contains(baseName);
            }

            private static string GetBaseAttributeName(NameSyntax name)
            {
                var text = name switch
                {
                    QualifiedNameSyntax qn => GetBaseAttributeName(qn.Right),
                    AliasQualifiedNameSyntax aqn => GetBaseAttributeName(aqn.Name),
                    SimpleNameSyntax sn => sn.Identifier.Text,
                    _ => name.ToString()
                };

                return TrimAttributeSuffix(text);
            }
        }
    }
}