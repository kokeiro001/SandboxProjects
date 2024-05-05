using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MarkdownDocumentGenerator
{
    public class TypeInfoCollector(Project project)
    {
        public async Task<TypeInfo[]> Collect(string targetBaseTypeName, int maxPropertyTypeCollectDepth = 3)
        {
            // プロジェクト内のすべてのソースコードファイルを取得
            var documents = project.Documents ?? [];

            var result = new List<TypeInfo>();

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
                    var typeSymbol = semanticModel.GetDeclaredSymbol(classSyntax);

                    if (typeSymbol is null)
                    {
                        continue;
                    }

                    // デバッグ用のパラメータが設定されている場合、任意の単一のクラス名以外はスキップする
                    if (!IsInheritType(typeSymbol, targetBaseTypeName))
                    {
                        continue;
                    }

                    Console.WriteLine($"{typeSymbol.Name} inherits from {targetBaseTypeName}");

                    var typeInfo = new TypeInfo(typeSymbol);
                    typeInfo.CollectProperties(maxPropertyTypeCollectDepth);
                    result.Add(typeInfo);
                }
            }

            return result.ToArray();
        }

        private static bool IsInheritType(INamedTypeSymbol classSymbol, string classFullName)
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
