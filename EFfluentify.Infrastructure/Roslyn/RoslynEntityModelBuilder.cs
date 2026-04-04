using EFfluentify.Application.Interfaces;
using EFfluentify.Domain.Models;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EFfluentify.Infrastructure.Roslyn
{
    public sealed class RoslynEntityModelBuilder : IEntityModelBuilder
    {
        private readonly IFileManager _fileSystemService;
        public RoslynEntityModelBuilder(IFileManager fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }
        public async Task<List<EntityModel>> BuildFromInputs(IEnumerable<string> inputs)
        {
            var files = _fileSystemService
                .ExpandFiles(inputs)
                .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase));
            var tasks = files.Select(async file =>
            {
                var text = await _fileSystemService.ReadFileAsync(file);
                return ParseFile(text, file);
            });

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(r => r).ToList();

        }

        private IEnumerable<EntityModel> ParseFile(string code, string sourcePath)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = tree.GetRoot() as CompilationUnitSyntax;
            if(root == null) yield break;

            var ns = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var clss in classes)
            {
                var entity = new EntityModel
                {
                    Namespace = ns,
                    Name = clss.Identifier.Text,
                    SourcePath = sourcePath
                };
                foreach (var attrList in clss.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        var attribute = new AttributeModel
                        {
                            Name = attr.Name.ToString(),
                            RawText = attr.ToString()
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

                        entity.Attributes.Add(attribute);
                    }
                }
                var properties = clss.DescendantNodes().OfType<PropertyDeclarationSyntax>();

                foreach (var prop in properties)
                {
                    var property = new PropertyModel
                    {
                        Name = prop.Identifier.Text,
                        TypeName = prop.Type.ToString(),
                        IsNullable = prop.Type.ToString().EndsWith("?")
                    };

                    var attributes = prop.AttributeLists;
                    foreach (var attrList in attributes)
                    {
                        foreach (var attr in attrList.Attributes)
                        {
                            var args = attr.ArgumentList;
                            var attribute = new AttributeModel
                            {
                                Name = attr.Name.ToString(),
                                RawText = attr.ToString()
                            };

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
                            property.Attributes.Add(attribute);
                        }
                    }
                    entity.Properties.Add(property);
                }
                yield return entity;
            }
        }
    }
}