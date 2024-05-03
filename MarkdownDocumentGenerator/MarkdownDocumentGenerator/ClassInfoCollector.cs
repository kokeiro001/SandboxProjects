using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MarkdownDocumentGenerator
{
    public class ClassInfoCollector(Project project)
    {
        public async Task<ClassInfo[]> Collect(string targetBaseClassName)
        {
            // プロジェクト内のすべてのソースコードファイルを取得
            var documents = project.Documents ?? [];

            var result = new List<ClassInfo>();

            // プロジェクト内の各ソースコードファイルに対して解析を実行
            foreach (var document in documents)
            {
                var syntaxTree = await document.GetSyntaxTreeAsync();
                var semanticModel = await document.GetSemanticModelAsync();

                if (syntaxTree is null || semanticModel is null)
                {
                    continue;
                }

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
                    if (!IsInheritClass(classSymbol, targetBaseClassName))
                    {
                        continue;
                    }

                    Console.WriteLine($"Class {classSymbol.Name} inherits from {targetBaseClassName}");

                    var classInfo = new ClassInfo(classSymbol);
                    classInfo.CollectProperties();
                    result.Add(classInfo);
                }
            }

            return result.ToArray();
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
