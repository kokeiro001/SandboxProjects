using Microsoft.CodeAnalysis;

namespace MarkdownDocumentGenerator
{
    public class ClassInfo
    {
        private readonly DocumentationComment documentationComment;

        public ClassInfo(INamedTypeSymbol classSymbol)
        {
            this.Symbol = classSymbol;

            var docComment = classSymbol.GetDocumentationCommentXml() ?? "";
            documentationComment = new DocumentationComment(docComment);
        }

        override public string ToString()
        {
            return FullName;
        }
        public INamedTypeSymbol Symbol { get; }

        public string DisplayName => Symbol.Name;

        public string Namespace => Symbol.ContainingNamespace?.ToString() ?? "";

        public string Summary => documentationComment.GetSummary();

        public string Remarks => documentationComment.GetRemarks();

        public List<PropertyInfo> Properties { get; set; } = [];

        public string FullName => string.IsNullOrEmpty(Namespace) ? DisplayName : $"{Namespace}.{DisplayName}";

        public List<ClassInfo> AssociationClasses { get; set; } = [];

        public void CollectProperties()
        {
            InternalCollectProperties(Symbol, AssociationClasses, 0);
        }

        private void InternalCollectProperties(INamedTypeSymbol baseSymbol, List<ClassInfo> associationClasses, int depth)
        {
            if (depth > 3)
            {
                return;
            }

            INamedTypeSymbol? currentSymbol = baseSymbol;

            // 継承元のクラスのプロパティも取得する
            while (currentSymbol != null)
            {
                foreach (var propertySymbol in currentSymbol.GetMembers().OfType<IPropertySymbol>())
                {
                    var propertyInfo = new PropertyInfo(propertySymbol);

                    Properties.Add(propertyInfo);
                }

                currentSymbol = currentSymbol.BaseType;
            }

            // プロパティとして取得した型がクラスか構造体の場合、関連クラスとして追加する
            foreach (var propertyInfo in Properties)
            {
                //  TODO: 構造体の場合も同様の処理でいける？ or TypeKind.Struct でいける？
                if (propertyInfo.Symbol.Type.TypeKind is TypeKind.Class)
                {
                    var namedTypoeSymbol = (INamedTypeSymbol)propertyInfo.Symbol.Type;

                    var classInfo = new ClassInfo(namedTypoeSymbol);

                    // 循環参照を防ぐため、すでに取得済みのクラスはスキップする
                    if (associationClasses.Any(x => x.FullName == classInfo.FullName))
                    {
                        continue;
                    }

                    // 同一アセンブリで定義されている独自のクラスのみ対象とする
                    if (AreContainingSameAssembly(baseSymbol.ContainingAssembly, propertyInfo.Symbol.Type.ContainingAssembly))
                    {
                        // この型を直接情報として追加する
                        associationClasses.Add(classInfo);
                        classInfo.InternalCollectProperties(classInfo.Symbol, associationClasses, depth + 1);
                    }

                    // List<T>とかジェネリックの場合、直接のNamespaceがSystemだったりするのでTの情報で判断する必要がある
                    var targetTypeArguments = namedTypoeSymbol.TypeArguments.OfType<INamedTypeSymbol>()
                        .Where(x => x.TypeKind is TypeKind.Class)
                        .Where(x => AreContainingSameAssembly(baseSymbol.ContainingAssembly, x.ContainingAssembly))
                        .ToArray();

                    foreach (var targetTypeArgument in targetTypeArguments)
                    {
                        var argumentClassInfo = new ClassInfo(targetTypeArgument);
                        if (associationClasses.Any(x => x.FullName == argumentClassInfo.FullName))
                        {
                            continue;
                        }

                        associationClasses.Add(argumentClassInfo);
                        argumentClassInfo.InternalCollectProperties(argumentClassInfo.Symbol, associationClasses, depth + 1);
                    }
                }
            }

            // TODO: プロパティとして取得した形がenumの場合、enumの値を取得する
            foreach (var propertyInfo in Properties)
            {
                if (propertyInfo.Symbol.Type.TypeKind == TypeKind.Enum)
                {
                    // implement
                }
            }
        }

        private static bool AreContainingSameAssembly(IAssemblySymbol left, IAssemblySymbol right)
        {
            // どっちもnullで同一判定になると困るのでnullチェック
            if (left is null || right is null)
            {
                return false;
            }

            var result = SymbolEqualityComparer.Default.Equals(left, right);

            return result;
        }
    }
}
