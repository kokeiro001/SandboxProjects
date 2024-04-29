namespace MarkdownDocumentGenerator.Test
{
    public class GetProjectPathSandboxTest
    {
        [Fact]
        public void 相対パスでデバッグ用のプロジェクトを取得するテスト1()
        {
            var currentDirectory = new Uri(Environment.CurrentDirectory, UriKind.Absolute);
            var relative = new Uri("../../../MarkdownDocumentGenerator/MarkdownDocumentGenerator.csproj", UriKind.Relative);

            var projectPath = new Uri(currentDirectory, relative);

            var existsProject = File.Exists(projectPath.AbsolutePath);

            Assert.True(existsProject);
        }

        [Fact]
        public void 相対パスでデバッグ用のプロジェクトを取得するテスト2()
        {
            var currentDirectory = new Uri(Environment.CurrentDirectory, UriKind.Absolute);
            var relative = new Uri("../../../DTO/DTO.csproj", UriKind.Relative);

            var projectPath = new Uri(currentDirectory, relative);

            var existsProject = File.Exists(projectPath.AbsolutePath);

            Assert.True(existsProject);
        }
    }
}
