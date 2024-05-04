using LibGit2Sharp;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Configuration;
using RazorEngineCore;

namespace MarkdownDocumentGenerator
{
    public class Program
    {
        static async Task Main()
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

            //classInfos.DumpConsole();

            var repositoryPath = Repository.Discover(Path.GetDirectoryName(config.ProjectPath));

            foreach (var classInfo in classInfos)
            {
                var gitRepositoryInfo = GetRepositoryInfo(repositoryPath);

                var renderMarkdownModel = new RenderMarkdownModel(classInfo)
                {
                    RenderProjectGitBranch = gitRepositoryInfo.branch,
                    RenderProjectGitCommitHash = gitRepositoryInfo.lastCommitHash,
                };

                var outputMarkdownFilepath = Path.Combine(config.OutputMarkdownDirectory, $"{classInfo.DisplayName}.md");

                await RenderMarkdown(renderMarkdownModel, outputMarkdownFilepath);
            }
        }

        static async Task RenderMarkdown(
            RenderMarkdownModel renderMarkdownModel,
            string outputMarkdownFilepath)
        {
            var markdownTemplate = await File.ReadAllTextAsync("MarkdownTemplate.cshtml");
            var razorEngine = new RazorEngine();
            var template = razorEngine.Compile(markdownTemplate);

            var result = await template.RunAsync(renderMarkdownModel);
            await File.WriteAllTextAsync(outputMarkdownFilepath, result);

            Console.WriteLine($"Finish RenderMarkdown {renderMarkdownModel.ClassInfo.DisplayName}");
        }

        public class RenderMarkdownModel(ClassInfo classInfo)
        {
            public ClassInfo ClassInfo { get; } = classInfo;

            public DateTimeOffset RenderDateTime { get; init; } = DateTimeOffset.Now;

            public string RenderProjectGitBranch { get; init; } = "";

            public string RenderProjectGitCommitHash { get; init; } = "";
        }

        private static (string branch, string lastCommitHash) GetRepositoryInfo(string repositoryPath)
        {
            using var repo = new Repository(repositoryPath);
            // 現在のブランチ名を取得
            var currentBranch = repo.Head.FriendlyName;

            // 最新のコミットを取得
            Commit latestCommit = repo.Head.Tip;

            return (currentBranch, latestCommit.Sha);
        }
    }
}
