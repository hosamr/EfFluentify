using EFfluentify.Application.Models;
using EFfluentify.Application.Interfaces;
using EFfluentify.Domain.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data;

namespace EFfluentify.Infrastructure.Roslyn
{
    public class AnnotationRemover : IAnnotationRemover
    {
        private IFileManager _fileSystemService;

        public AnnotationRemover(IFileManager fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }

        public async Task<IReadOnlyList<AnnotationRemovalChange>> PrepareRemovalAsync(IEnumerable<string> inputs)
        {
            var rules = RuleRegistry.Default();
            var files = _fileSystemService.ExpandFiles(inputs)
                         .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));

            var targets = rules.AllAnnotationAttributeNames()
                .Select(n => n.EndsWith("Attribute", StringComparison.Ordinal) ? n[..^9] : n)
                .ToHashSet(StringComparer.Ordinal);

            var results = new List<AnnotationRemovalChange>();
            foreach (var file in files)
            {
                var original = await _fileSystemService.ReadFileAsync(file);
                var tree = CSharpSyntaxTree.ParseText(original);
                var root = tree.GetCompilationUnitRoot();

                var rewriter = new AnnotationStripper(targets);
                var newRoot = (CompilationUnitSyntax)rewriter.Visit(root);

                newRoot = RemoveEmptyAttributeLists(newRoot);

                var updated = newRoot.NormalizeWhitespace().ToFullString();

                if (!string.Equals(original, updated, StringComparison.Ordinal))
                {
                    results.Add(new AnnotationRemovalChange(file, original, updated));
                }
            }
            return results;
        }

        private CompilationUnitSyntax RemoveEmptyAttributeLists(CompilationUnitSyntax root)
        {
            var rewriter = new EmptyAttrListCleaner();
            return (CompilationUnitSyntax)rewriter.Visit(root);
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
                // Filter attributes in this list
                var kept = new SeparatedSyntaxList<AttributeSyntax>();
                foreach (var attr in node.Attributes)
                {
                    if (!IsTarget(attr))
                        kept = kept.Add(attr);
                }

                // If nothing left in this list, remove the whole list
                if (kept.Count == 0) return null;

                return node.WithAttributes(kept);
            }

                private bool IsTarget(AttributeSyntax attr)
                {
                    // attr.Name could be "Required", "RequiredAttribute", "Schema.Column", "EF.Index", etc.
                    var last = attr.Name switch
                    {
                        QualifiedNameSyntax q => q.Right.Identifier.Text,
                        IdentifierNameSyntax id => id.Identifier.Text,
                        GenericNameSyntax g => g.Identifier.Text,
                        _ => attr.Name.ToString()
                    };

                    // strip trailing "Attribute" if present
                    if (last.EndsWith("Attribute", StringComparison.Ordinal))
                        last = last[..^9];

                    return _targets.Contains(last);
                }
        }

        private sealed class EmptyAttrListCleaner : CSharpSyntaxRewriter
        {
            public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
                => base.VisitClassDeclaration(Clean(node))!;
            public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
                => base.VisitPropertyDeclaration(Clean(node))!;
            public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
                => base.VisitFieldDeclaration(Clean(node))!;
            public override SyntaxNode? VisitParameter(ParameterSyntax node)
                => base.VisitParameter(Clean(node))!;

            private T Clean<T>(T node) where T : SyntaxNode
            {
                if (node is null) return node!;
                if (node is not CSharpSyntaxNode cs) return node;

                if (cs is MemberDeclarationSyntax m && m.AttributeLists.Count > 0)
                {
                    var keep = new SyntaxList<AttributeListSyntax>();
                    foreach (var l in m.AttributeLists)
                        if (l.Attributes.Count > 0) keep = keep.Add(l);

                    return (T)(SyntaxNode)m.WithAttributeLists(keep);
                }

                if (cs is ParameterSyntax p && p.AttributeLists.Count > 0)
                {
                    var keep = new SyntaxList<AttributeListSyntax>();
                    foreach (var l in p.AttributeLists)
                        if (l.Attributes.Count > 0) keep = keep.Add(l);

                    return (T)(SyntaxNode)p.WithAttributeLists(keep);
                }

                return node;
            }
        }
    }

}