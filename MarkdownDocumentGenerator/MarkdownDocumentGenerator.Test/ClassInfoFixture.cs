using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace MarkdownDocumentGenerator.Test
{
    public class ClassInfoFixture
    {
        public ClassInfo[] ClassInfos { get; }

        public ClassInfoFixture()
        {
            var currentDirectory = new Uri(Environment.CurrentDirectory, UriKind.Absolute);
            var relative = new Uri("../../../DTO/DTO.csproj", UriKind.Relative);
            var projectPath = new Uri(currentDirectory, relative);

            MSBuildLocator.RegisterDefaults();
            using var workspace = MSBuildWorkspace.Create();

            var project = workspace.OpenProjectAsync(projectPath.AbsolutePath).Result;

            var classInfoCollector = new ClassInfoCollector(project);

            ClassInfos = classInfoCollector.Collect("DTO.DTOBase").Result;
        }
    }
}
