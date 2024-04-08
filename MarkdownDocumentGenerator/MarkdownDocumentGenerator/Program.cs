using Dumpify;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Configuration;
using RazorEngineCore;

namespace MarkdownDocumentGenerator
{
    static class GlobalCache
    {
        public static INamedTypeSymbol? ListTypeSymbol { get; set; }
    }

    public static class Constants
    {
        public static readonly string TargetBaseClassName = "DTO.DTOBase";
        public static readonly string TargetBaseNamespace = "DTO";
    }

    internal class Program
    {

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

                    if (!IsInheritClass(classSymbol, Constants.TargetBaseClassName))
                    {
                        continue;
                    }

                    Console.WriteLine($"Class {classSymbol.Name} inherits from {Constants.TargetBaseClassName}");

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
}
