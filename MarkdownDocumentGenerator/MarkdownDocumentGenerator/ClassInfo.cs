using Microsoft.CodeAnalysis;

namespace MarkdownDocumentGenerator
{
    public class ClassInfo
    {
        private readonly DocumentationComment documentationComment;

        public ClassInfo(INamedTypeSymbol classSymbol)
        {
            Symbol = classSymbol;

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

        public IReadOnlyList<PropertyInfo> Properties => properties;
        private readonly List<PropertyInfo> properties = [];

        public string FullName => string.IsNullOrEmpty(Namespace) ? DisplayName : $"{Namespace}.{DisplayName}";

        public IReadOnlyList<ClassInfo> AssociationClasses => associationClasses;
        private readonly List<ClassInfo> associationClasses = [];

        public IReadOnlyList<EnumInfo> AssociationEnums => associationEnums;
        private readonly List<EnumInfo> associationEnums = [];

        public void CollectProperties(int maxDepth)
        {
            // 再帰的に呼び出す
            InternalCollectProperties(Symbol, 0, maxDepth);
        }

        private void InternalCollectProperties(INamedTypeSymbol baseSymbol, int currentDepth, int maxDepth)
        {
            if (currentDepth > maxDepth)
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

                    properties.Add(propertyInfo);
                }

                currentSymbol = currentSymbol.BaseType;
            }

            // プロパティとして取得した型がクラスか構造体の場合、関連クラスとして追加する
            foreach (var propertyInfo in properties)
            {
                switch (propertyInfo.Symbol.Type.TypeKind)
                {
                    case TypeKind.Array:
                        HandleArray(propertyInfo.Symbol.Type, baseSymbol, currentDepth, maxDepth);
                        break;
                    case TypeKind.Class:
                    case TypeKind.Struct:
                        HandleClassOrStruct(propertyInfo.Symbol.Type, baseSymbol, currentDepth, maxDepth);
                        break;
                    case TypeKind.Enum:
                        HandleEnum(baseSymbol, propertyInfo);
                        break;
                }
            }
        }

        private void HandleArray(ITypeSymbol propertyTypeSymbol, INamedTypeSymbol baseSymbol, int currentDepth, int maxDepth)
        {
            var arrayTypeSymbol = (IArrayTypeSymbol)propertyTypeSymbol;
            var propertyNamedTypeSymbol = (INamedTypeSymbol)arrayTypeSymbol.ElementType;

            HandleClassOrStruct(propertyNamedTypeSymbol, baseSymbol, currentDepth, maxDepth);

            // FIXME: Enumの場合にうまく動かないので修正する
        }

        private void HandleClassOrStruct(ITypeSymbol propertyTypeSymbol, INamedTypeSymbol baseSymbol, int currentDepth, int maxDepth)
        {
            var namedTypoeSymbol = (INamedTypeSymbol)propertyTypeSymbol;

            var classInfo = new ClassInfo(namedTypoeSymbol);

            // 循環参照を防ぐため、すでに取得済みのクラスはスキップする
            if (associationClasses.Any(x => x.FullName == classInfo.FullName))
            {
                return;
            }

            // 同一アセンブリで定義されている独自のクラスのみ対象とする
            if (AreContainingSameAssembly(baseSymbol.ContainingAssembly, propertyTypeSymbol.ContainingAssembly))
            {
                // この型を直接情報として追加する
                associationClasses.Add(classInfo);
                classInfo.InternalCollectProperties(classInfo.Symbol, currentDepth + 1, maxDepth);
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
                argumentClassInfo.InternalCollectProperties(argumentClassInfo.Symbol, currentDepth + 1, maxDepth);
            }
        }

        private void HandleEnum(INamedTypeSymbol baseSymbol, PropertyInfo propertyInfo)
        {
            var namedTypoeSymbol = (INamedTypeSymbol)propertyInfo.Symbol.Type;

            var enumInfo = new EnumInfo(namedTypoeSymbol);

            // 循環参照を防ぐため、すでに取得済みのenumはスキップする
            if (associationEnums.Any(x => x.FullName == enumInfo.FullName))
            {
                return;
            }

            // 同一アセンブリで定義されている独自のenumのみ対象とする
            if (AreContainingSameAssembly(baseSymbol.ContainingAssembly, propertyInfo.Symbol.Type.ContainingAssembly))
            {
                // この型を直接情報として追加する
                associationEnums.Add(enumInfo);
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
