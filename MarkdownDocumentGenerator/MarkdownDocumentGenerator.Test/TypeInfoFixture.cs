using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace MarkdownDocumentGenerator.Test
{
    public class TypeInfoFixture
    {
        public TypeInfo[] TypeInfos { get; }

        public TypeInfoFixture()
        {
            var currentDirectory = new Uri(Environment.CurrentDirectory, UriKind.Absolute);
            var relative = new Uri("../../../DTO/DTO.csproj", UriKind.Relative);
            var projectPath = new Uri(currentDirectory, relative);

            MSBuildLocator.RegisterDefaults();
            using var workspace = MSBuildWorkspace.Create();

            var project = workspace.OpenProjectAsync(projectPath.AbsolutePath).Result;

            var typeInfoCollector = new TypeInfoCollector(project);

            TypeInfos = typeInfoCollector.Collect("DTO.DTOBase").Result;
        }
    }
}
