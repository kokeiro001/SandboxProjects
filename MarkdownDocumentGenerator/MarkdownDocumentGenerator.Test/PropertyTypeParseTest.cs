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
        [InlineData("DTO.GameInfo", "Title", "string")]
        [InlineData("DTO.GameInfo", "PlayersArray", "Player[]")]
        [InlineData("DTO.GameInfo", "PlayersList", "List<Player>")]
        [InlineData("DTO.GameInfo", "Item1", "Item1?")]
        [InlineData("DTO.GameInfo", "Item2", "Item2?")]
        [InlineData("DTO.GameInfo", "Item3", "Item3?")]
        [InlineData("DTO.RequestFood", "FoodName", "string")]
        [InlineData("DTO.GodPlayer", "Rank", "int")]
        [InlineData("DTO.GodPlayer", "CurrentMoveType", "MoveType")]
        [InlineData("DTO.CircularReferencePlayer", "Parent", "CircularReferencePlayer?")]
        [InlineData("DTO.CircularReferencePlayer", "Children", "CircularReferencePlayer[]")]
        [InlineData("DTO.GodGameInfo", "WorldId", "int")]
        [InlineData("DTO.NullableDTO", "NullableInt", "int?")]
        [InlineData("DTO.NullableDTO", "NullableDateTime", "DateTime?")]
        [InlineData("DTO.NullableDTO", "NullableString", "string?")]
        [InlineData("DTO.NullableDTO", "NullableItem1", "Item1?")]
        [InlineData("DTO.NullableDTO", "IntArray", "int[]")]
        [InlineData("DTO.NullableDTO", "IntNullableArray", "int[]?")]
        [InlineData("DTO.NullableDTO", "NullableIntArray", "int?[]")]
        [InlineData("DTO.NullableDTO", "NullableIntNullableArray", "int?[]?")]
        [InlineData("DTO.NullableDTO", "IntList", "List<int>")]
        [InlineData("DTO.NullableDTO", "IntNullableList", "List<int>?")]
        [InlineData("DTO.NullableDTO", "NullableIntList", "List<int?>")]
        [InlineData("DTO.NullableDTO", "NullableIntNullableList", "List<int?>?")]
        [InlineData("DTO.NullableDTO", "NullableIntNullableListNullableList", "List<List<int?>?>?")]
        public void PropertyTypeParse(string fullClassName, string propertyDisplayName, string expectedDisplayTypeName)
        {
            var targetClassInfo = classInfoFixture.ClassInfos
                .FirstOrDefault(x => x.FullName == fullClassName);

            Assert.NotNull(targetClassInfo);

            var targetPropertyInfo = targetClassInfo.Properties
                .FirstOrDefault(x => x.DisplayName == propertyDisplayName);

            Assert.NotNull(targetPropertyInfo);

            Assert.Equal(expectedDisplayTypeName, targetPropertyInfo.DisplayTypeName);
        }
    }
}
