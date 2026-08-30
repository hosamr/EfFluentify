using EFFluentify.Application.Interfaces;
using EFFluentify.Domain.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EFFluentify.Infrastructure.Roslyn
{
    public sealed class RoslynEntityModelBuilder : IEntityModelBuilder
    {
        private readonly IFileManager _fileManager;
        public RoslynEntityModelBuilder(IFileManager fileManager)
        {
            _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        }
        public async Task<List<EntityModel>> BuildFromInputsAsync(IEnumerable<string> inputs)
        {
            var files = _fileManager
                .ExpandFiles(inputs)
                .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
            var tasks = files.Select(async file =>
            {
                var text = await _fileManager.ReadFileAsync(file);
                return ParseFile(text);
            });

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(r => r).ToList();

        }

        private IEnumerable<EntityModel> ParseFile(string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot() as CompilationUnitSyntax;
            if (root == null) yield break;

            var ns = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var clss in classes)
            {
                yield return ParseClass(clss, ns);
            }
        }

        private EntityModel ParseClass(ClassDeclarationSyntax clss, string ns)
        {
            var entity = new EntityModel
            {
                Namespace = ns,
                Name = clss.Identifier.Text
            };

            var properties = clss.DescendantNodes().OfType<PropertyDeclarationSyntax>();
            foreach (var prop in properties)
            {
                entity.Properties.Add(ParseProperty(prop));
            }

            entity.Attributes.AddRange(ParseAttributes(clss.AttributeLists));

            return entity;
        }

        private Property ParseProperty(PropertyDeclarationSyntax prop)
        {
            var typeName = prop.Type.ToString();
            var property = new Property
            {
                Name = prop.Identifier.Text,
                TypeName = typeName,
                IsNullable = typeName.EndsWith("?")
            };

            property.Attributes.AddRange(ParseAttributes(prop.AttributeLists));

            return property;
        }

        private IEnumerable<AttributeEntry> ParseAttributes(SyntaxList<AttributeListSyntax> attributeLists)
        {
            foreach (var attrList in attributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var attribute = new AttributeEntry
                    {
                        Name = attr.Name.ToString()
                    };

                    var args = attr.ArgumentList;
                    if (args != null)
                    {
                        foreach (var arg in args.Arguments)
                        {
                            if (arg.NameEquals != null)
                            {
                                attribute.NamedArgs[arg.NameEquals.Name.ToString()] = arg.Expression.ToString();
                            }
                            else
                            {
                                attribute.PositionalArgs.Add(arg.Expression.ToString());
                            }
                        }
                    }

                    yield return attribute;
                }
            }
        }
    }
}