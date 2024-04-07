using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Dumpify;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Configuration;
using RazorEngineCore;
using Spectre.Console;

namespace MarkdownDocumentGenerator
{
    internal class Program
    {
        static readonly string TargetBaseClassName = "DTO.DTOBase";

        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();

            var dtoProjectFilePath = configuration["DTOProjectPath"] ?? "";
            var outputMarkdownDirectory = configuration["OutputMarkdownDirectory"] ?? "";

            if (!Directory.Exists(outputMarkdownDirectory))
            {
                Directory.CreateDirectory(outputMarkdownDirectory);
            }

            MSBuildLocator.RegisterDefaults();
            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(dtoProjectFilePath);

            // プロジェクト内のすべてのソースコードファイルを取得
            var documents = project.Documents ?? [];

            var classInfos = new List<ClassInfo>();

            // プロジェクト内の各ソースコードファイルに対して解析を実行
            foreach (var document in documents)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();
                var semanticModel = await document.GetSemanticModelAsync();

                if (syntaxTree is null || semanticModel is null)
                {
                    continue;
                }

                // これって使いまわしていいものなのか？
                // 同一プロジェクト内ならセーフな気がするが、document/semanticModelごとに変わったりする？わからん。
                GlobalCache.ListTypeSymbol ??= semanticModel.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");

                // 解析して特定のクラスを継承しているクラスの一覧を取得
                var root = syntaxTree.GetCompilationUnitRoot();
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

                foreach (var classSyntax in classes)
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax);

                    if (classSymbol is null)
                    {
                        continue;
                    }

                    if (!IsInheritClass(classSymbol, TargetBaseClassName))
                    {
                        continue;
                    }

                    Console.WriteLine($"Class {classSymbol.Name} inherits from {TargetBaseClassName}");

                    var classInfo = new ClassInfo(classSymbol);
                    classInfo.CollectProperties();
                    classInfos.Add(classInfo);
                }
            }

            classInfos.DumpConsole();

            foreach (var classInfo in classInfos)
            {
                await RenderMarkdown(classInfo, outputMarkdownDirectory);
            }
        }

        static async Task RenderMarkdown(ClassInfo classInfo, string outputMarkdownDirectory)
        {
            var outputMarkdownFilepath = Path.Combine(outputMarkdownDirectory, $"{classInfo.DisplayName}.md");

            var markdownTemplate = await File.ReadAllTextAsync("MarkdownTemplate.cshtml");
            var razorEngine = new RazorEngine();
            var template = razorEngine.Compile(markdownTemplate);

            var result = await template.RunAsync(classInfo);
            await File.WriteAllTextAsync(outputMarkdownFilepath, result);
            Console.WriteLine(result);
        }

        private static bool IsInheritClass(INamedTypeSymbol classSymbol, string classFullName)
        {
            INamedTypeSymbol? currentSymbol = classSymbol;

            while (currentSymbol.BaseType != null)
            {
                if (currentSymbol.BaseType.ToString() == classFullName)
                {
                    return true;
                }

                currentSymbol = currentSymbol.BaseType;
            }

            return false;
        }
    }

    public class DocumentationComment
    {
        private readonly IHtmlDocument? document;

        public DocumentationComment(string xml)
        {
            try
            {
                var parser = new HtmlParser();
                document = parser.ParseDocument(xml);
            }
            catch
            {
            }
        }

        public string GetSummary()
        {
            return GetTagText("summary");
        }

        public string GetRemarks()
        {
            return GetTagText("remarks");
        }

        private string GetTagText(string tag)
        {
            if (document is null)
            {
                return "";
            }

            var tagEelement = document.QuerySelector(tag);

            if (tagEelement is null)
            {
                return "";
            }

            // seeタグは中身のcref, hrefをテキストとして書き起こす
            var seeElements = tagEelement.GetElementsByTagName("see");
            var seealsoElements = tagEelement.GetElementsByTagName("seealso");

            var replaceSeeElements = seeElements.Concat(seealsoElements);

            foreach (var seeElement in replaceSeeElements)
            {
                var newElement = document.CreateElement("span");
                newElement.TextContent = seeElement.GetAttribute("cref") ?? seeElement.GetAttribute("href") ?? "";

                // なんか直接seeElementを置き換えてもだめだったのでインサートしている
                tagEelement.InsertBefore(newElement, seeElement);
            }

            var elementValue = tagEelement.TextContent.Trim();

            var trimLines = elementValue
                .Split('\n')
                .Select(x => x.Trim());

            return string.Join('\n', trimLines);
        }
    }

    public class ClassInfo
    {
        private readonly DocumentationComment documentationComment;
        private readonly INamedTypeSymbol classSymbol;

        public ClassInfo(INamedTypeSymbol classSymbol)
        {
            this.classSymbol = classSymbol;
            var docComment = classSymbol.GetDocumentationCommentXml() ?? "";
            documentationComment = new DocumentationComment(docComment);
        }

        public string DisplayName => classSymbol.Name;

        public string Namespace => classSymbol.ContainingNamespace?.ToString() ?? "";

        public string Summary => documentationComment.GetSummary();

        public string Remarks => documentationComment.GetRemarks();

        public List<PropertyInfo> Properties { get; set; } = [];

        public string FullName => string.IsNullOrEmpty(Namespace) ? DisplayName : $"{Namespace}.{DisplayName}";

        public List<ClassInfo> AssociationClasses { get; set; } = [];

        public void CollectProperties()
        {
            INamedTypeSymbol? currentClassSymbol = classSymbol;

            while (currentClassSymbol != null)
            {
                foreach (var propertySymbol in currentClassSymbol.GetMembers().OfType<IPropertySymbol>())
                {
                    var propertyInfo = new PropertyInfo(propertySymbol);

                    Properties.Add(propertyInfo);
                }

                currentClassSymbol = currentClassSymbol.BaseType;
            }

        }
    }

    static class GlobalCache
    {
        public static INamedTypeSymbol? ListTypeSymbol { get; set; }
    }

    public class PropertyInfo
    {
        private readonly IPropertySymbol propertySymbol;
        private readonly DocumentationComment documentationComment;

        public PropertyInfo(IPropertySymbol propertySymbol)
        {
            this.propertySymbol = propertySymbol;

            var propertyDocumentationCommentXml = propertySymbol.GetDocumentationCommentXml() ?? "";

            documentationComment = new DocumentationComment(propertyDocumentationCommentXml);
        }

        public string DisplayName => propertySymbol.Name;

        public string DisplayTypeName => GetTypeName();

        public string Summary => documentationComment.GetSummary();
        public string Remarks => documentationComment.GetRemarks();

        private string GetTypeName()
        {
            if (IsListType())
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

        private bool IsListType()
        {
            return propertySymbol.Type.OriginalDefinition.Equals(GlobalCache.ListTypeSymbol, SymbolEqualityComparer.Default)
                   && propertySymbol.Type is INamedTypeSymbol namedType
                   && namedType.IsGenericType
                   && namedType.TypeArguments.Length == 1;
        }
    }
}
