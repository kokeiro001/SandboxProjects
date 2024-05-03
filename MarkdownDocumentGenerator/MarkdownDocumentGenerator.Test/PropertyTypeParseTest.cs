using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace MarkdownDocumentGenerator.Test
{
    public class ClassInfoFixture : IDisposable
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

        public void Dispose()
        {
        }
    }

    public class PropertyTypeParseTest(ClassInfoFixture classInfoFixture) : IClassFixture<ClassInfoFixture>
    {
        [Theory]
        [InlineData("DTO.Player", "Name", "string")]
        [InlineData("DTO.Player", "Hp", "int")]
        public void PropertyTypeParse(string fullClassName, string propertyDisplayName, string expectedDisplayTypeName)
        {
            var targetClassInfo = classInfoFixture.ClassInfos
                .First(x => x.FullName == fullClassName);

            var targetPropertyInfo = targetClassInfo.Properties
                .First(x => x.DisplayName == propertyDisplayName);

            Assert.Equal(expectedDisplayTypeName, targetPropertyInfo.DisplayTypeName);
        }
    }
}
