using Dumpify;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
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

            var classInfoCollector = new ClassInfoCollector(project);

            var classInfos = await classInfoCollector.Collect(config.TargetBaseClassName);

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
    }
}
