using System.Xml.Linq;
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

                if (syntaxTree is null)
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
                    if (classSymbol?.BaseType?.Name == "DTOBase")
                    {
                        Console.WriteLine($"Class {classSymbol.Name} inherits from {classSymbol.BaseType.Name}");

                        var docComment = classSymbol.GetDocumentationCommentXml() ?? "";
                        var classDocComment = new DocumentationComment(docComment);

                        Console.WriteLine($"Documentation for class {classSymbol.Name}: {classDocComment.GetSummary()}");

                        foreach (var member in classSymbol.GetMembers())
                        {
                            var memberDocumentationCommentXml = member.GetDocumentationCommentXml();

                            if (string.IsNullOrEmpty(memberDocumentationCommentXml))
                            {
                                continue;
                            }

                            var memberDocComment = new DocumentationComment(memberDocumentationCommentXml);

                            Console.WriteLine($"Documentation for member {member.Name}: Summary: {memberDocComment.GetSummary()}");
                        }
                    }
                }
            }
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

    class TypeInfo
    {

    }
}
