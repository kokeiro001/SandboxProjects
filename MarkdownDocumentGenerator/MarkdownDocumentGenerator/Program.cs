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

    public class Program
    {
        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<Program>()
                .Build()
                .Get<Config>();

            Config.Validate(config);

            if (!Directory.Exists(config.OutputMarkdownDirectory))
            {
                Directory.CreateDirectory(config.OutputMarkdownDirectory);
            }

            MSBuildLocator.RegisterDefaults();
            using var workspace = MSBuildWorkspace.Create();
            var project = await workspace.OpenProjectAsync(config.ProjectPath);

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

                    // デバッグ用のパラメータが設定されている場合、任意の単一のクラス名以外はスキップする
                    if (!string.IsNullOrEmpty(config.Debug?.TargetClassName))
                    {
                        if (classSymbol.ToString() != config.Debug.TargetClassName)
                        {
                            continue;
                        }

                        Console.WriteLine($"Class {classSymbol.Name} is {config.Debug.TargetClassName}");
                    }
                    else
                    {
                        if (!IsInheritClass(classSymbol, config.TargetBaseClassName))
                        {
                            continue;
                        }

                        Console.WriteLine($"Class {classSymbol.Name} inherits from {config.TargetBaseClassName}");
                    }

                    var classInfo = new ClassInfo(classSymbol, config);
                    classInfo.CollectProperties();
                    classInfos.Add(classInfo);
                }
            }

            classInfos.DumpConsole();

            foreach (var classInfo in classInfos)
            {
                await RenderMarkdown(classInfo, config.OutputMarkdownDirectory);
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
