using System.Xml.Linq;
using Dumpify;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Configuration;

namespace MarkdownDocumentGenerator
{
    internal class Program
    {
        static readonly string TargetBaseClassName = "DTOBase";

        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var dtoProjectFilePath = configuration["DTOProjectPath"];

            MSBuildLocator.RegisterDefaults();
            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(dtoProjectFilePath);


            // プロジェクト内のすべてのソースコードファイルを取得
            var documents = project.Documents ?? [];

            var classInfos = new List<ClassInfo>();

            // プロジェクト内の各ソースコードファイルに対して解析を実行
            foreach (var document in documents)
            {
                if (document is null)
                {
                    continue;
                }


                // SyntaxTreeを取得してソースコードを解析
                var syntaxTree = await document.GetSyntaxTreeAsync();
                var semanticModel = await document.GetSemanticModelAsync();

                if (syntaxTree is null || semanticModel is null)
                {
                    continue;
                }

                // 解析して特定のクラスを継承しているクラスの一覧を取得
                var root = syntaxTree.GetCompilationUnitRoot();
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classSyntax in classes)
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax);

                    // TODO: 名前空間込みの名前にする
                    if (classSymbol?.BaseType?.Name == TargetBaseClassName)
                    {
                        Console.WriteLine($"Class {classSymbol.Name} inherits from {classSymbol.BaseType.Name}");
                        var docComment = classSymbol.GetDocumentationCommentXml() ?? "";
                        var classDocComment = new DocumentationComment(docComment);

                        var classInfo = new ClassInfo
                        {
                            Name = classSymbol.Name ?? "",
                            Namespace = classSymbol.ContainingNamespace?.ToString() ?? "",
                            Summary = classDocComment.GetSummary(),
                        };

                        Console.WriteLine($"Documentation for class {classInfo.Name}: {classInfo.Summary}");

                        foreach (var propertySymbol in classSymbol.GetMembers().OfType<IPropertySymbol>())
                        {
                            var propertyDocumentationCommentXml = propertySymbol.GetDocumentationCommentXml();

                            if (string.IsNullOrEmpty(propertyDocumentationCommentXml))
                            {
                                continue;
                            }

                            var propertyDocComment = new DocumentationComment(propertyDocumentationCommentXml);

                            var propertyInfo = new PropertyInfo
                            {
                                Name = propertySymbol.Name,
                                TypeName = GetTypeName(propertySymbol, semanticModel),
                                Summary = propertyDocComment.GetSummary(),
                            };

                            Console.WriteLine($"Documentation for member {propertyInfo.Name}: Summary: {propertyInfo.Summary}");

                            classInfo.Properties.Add(propertyInfo);
                        }

                        classInfos.Add(classInfo);
                    }
                }
            }

            classInfos.DumpConsole();
        }

        private static string GetTypeName(IPropertySymbol propertySymbol, SemanticModel semanticModel)
        {
            var listTypeSymbol = semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");

            if (IsListType(propertySymbol.Type, listTypeSymbol!))
            {
                var firstTypeArgument = ((INamedTypeSymbol)propertySymbol.Type).TypeArguments.First();
                var typeName = firstTypeArgument.Name;

                return $"List<{typeName}>";
            }

            if (propertySymbol.Type.Kind == SymbolKind.ArrayType
                && propertySymbol.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                return $"{arrayTypeSymbol.ElementType.Name}[]";
            }

            return propertySymbol.Type.Name;
        }

        static bool IsListType(ITypeSymbol typeSymbol, INamedTypeSymbol listTypeSymbol)
        {
            // 型シンボルが List<T> かどうかを判断
            return typeSymbol.OriginalDefinition.Equals(listTypeSymbol, SymbolEqualityComparer.Default)
                   && typeSymbol is INamedTypeSymbol namedType
                   && namedType.IsGenericType
                   && namedType.TypeArguments.Length == 1;
        }
    }

    class DocumentationComment
    {
        private readonly XElement xmlElement;

        public DocumentationComment(string xml)
        {
            xmlElement = XElement.Parse(xml);
        }

        public string GetSummary()
        {
            return GetTag("summary");
        }

        private string GetTag(string tag)
        {
            return xmlElement.Element(tag)?.Value?.Trim() ?? "";
        }
    }

    class ClassInfo
    {
        public string Name { get; set; } = "";

        public string Namespace { get; set; } = "";

        public string Summary { get; set; } = "";

        public List<PropertyInfo> Properties { get; set; } = [];

        public string FullName => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";
    }

    class PropertyInfo
    {
        public string Name { get; set; } = "";

        public string TypeName { get; set; } = "";

        public string Summary { get; set; } = "";
    }
}
