using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;

namespace MarkdownDocumentGeneratorByBuildalyzer
{
    internal class Program
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


            var manager = new AnalyzerManager();
            manager.GetProject(config.ProjectPath);

            var workspace = manager.GetWorkspace();
            var targetProject = workspace.CurrentSolution.Projects.First();

            var compilation = await targetProject.GetCompilationAsync();

            if (compilation is null)
            {
                return;
            }

            Console.WriteLine(compilation.GlobalNamespace);


            var targetSymbol = compilation.GetTypeByMetadataName(config.TargetBaseClassName);

            var baseClassName = config.TargetBaseClassName;

            // 対象クラスのシンボルを取得
            var baseClassSymbol = compilation.GetTypeByMetadataName(baseClassName);

            if (baseClassSymbol == null)
            {
                return;
            }

            // 継承関係を持つクラスを検索
            var derivedClasses = compilation.GetSymbolsWithName(name =>
            {
                var symbol = compilation.GetTypeByMetadataName(name);
                return symbol != null && symbol.BaseType != null && SymbolEqualityComparer.Default.Equals(symbol, baseClassSymbol);
            }, SymbolFilter.Type)
            .ToArray();

            foreach (INamedTypeSymbol derivedClass in derivedClasses)
            {
                // シンボルからXMLドキュメントコメントを取得
                var xmlDocumentation = derivedClass.GetDocumentationCommentXml();

                // XMLドキュメントコメントを表示
                Console.WriteLine($"Derived class: {derivedClass.Name}");
                Console.WriteLine($"Documentation: {xmlDocumentation}");
            }

        }
    }
}
