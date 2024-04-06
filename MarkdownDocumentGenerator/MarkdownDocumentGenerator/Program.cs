using Microsoft.Build.Locator;
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
        }
    }
}
