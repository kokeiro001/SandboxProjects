using Microsoft.CodeAnalysis;

namespace MarkdownDocumentGenerator
{
    public class TypeInfo
    {
        private readonly DocumentationComment documentationComment;

        public TypeInfo(INamedTypeSymbol classSymbol)
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

        /// <summary>
        /// 関連するクラス、構造体の一覧
        /// </summary>
        /// <remarks>
        /// Typeって名前だと解釈できる範囲が広すぎるので本当は狭めたいがいい名前がわからない。
        /// </remarks>
        public IReadOnlyList<TypeInfo> AssociationTypes => associationTypes;
        private readonly List<TypeInfo> associationTypes = [];

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

            // プロパティとして取得した型の関連情報を取得する
            foreach (var propertyInfo in properties)
            {
                HandleSymbolByTypeKind(propertyInfo.Symbol.Type, baseSymbol, currentDepth, maxDepth);
            }
        }

        private void HandleSymbolByTypeKind(ITypeSymbol handleSymbol, INamedTypeSymbol baseSymbol, int currentDepth, int maxDepth)
        {
            switch (handleSymbol.TypeKind)
            {
                case TypeKind.Array:
                    HandleArray(handleSymbol, baseSymbol, currentDepth, maxDepth);
                    break;
                case TypeKind.Class:
                case TypeKind.Struct:
                    HandleClassOrStruct(handleSymbol, baseSymbol, currentDepth, maxDepth);
                    break;
                case TypeKind.Enum:
                    HandleEnum(handleSymbol, baseSymbol);
                    break;
            }
        }

        private void HandleArray(ITypeSymbol propertyTypeSymbol, INamedTypeSymbol baseSymbol, int currentDepth, int maxDepth)
        {
            var arrayTypeSymbol = (IArrayTypeSymbol)propertyTypeSymbol;

            HandleSymbolByTypeKind(arrayTypeSymbol.ElementType, baseSymbol, currentDepth, maxDepth);
        }

        private void HandleClassOrStruct(ITypeSymbol propertyTypeSymbol, INamedTypeSymbol baseSymbol, int currentDepth, int maxDepth)
        {
            var namedTypeSymbol = (INamedTypeSymbol)propertyTypeSymbol;

            var typeInfo = new TypeInfo(namedTypeSymbol);

            // すでに取得済みのクラスはスキップする
            if (associationTypes.Any(x => x.FullName == typeInfo.FullName))
            {
                return;
            }

            // 同一アセンブリで定義されている独自のクラスのみ対象とする
            if (AreContainingSameAssembly(baseSymbol.ContainingAssembly, propertyTypeSymbol.ContainingAssembly))
            {
                // この型を直接情報として追加する
                associationTypes.Add(typeInfo);
                typeInfo.InternalCollectProperties(typeInfo.Symbol, currentDepth + 1, maxDepth);
            }

            // List<T>とかジェネリックの場合、直接のNamespaceがSystemだったりするのでTの情報で判断する必要がある
            var sameAssemblyTypeArgumentSymbols = namedTypeSymbol.TypeArguments.OfType<INamedTypeSymbol>()
                .Where(x => AreContainingSameAssembly(baseSymbol.ContainingAssembly, x.ContainingAssembly))
                .ToArray();

            foreach (var sameAssemblyTypeArgumentSymbol in sameAssemblyTypeArgumentSymbols)
            {
                HandleSymbolByTypeKind(sameAssemblyTypeArgumentSymbol, baseSymbol, currentDepth, maxDepth);
            }
        }

        private void HandleEnum(ITypeSymbol propertyTypeSymbol, INamedTypeSymbol baseSymbol)
        {
            var namedTypeSymbol = (INamedTypeSymbol)propertyTypeSymbol;

            var enumInfo = new EnumInfo(namedTypeSymbol);

            // すでに取得済みのenumはスキップする
            if (associationEnums.Any(x => x.FullName == enumInfo.FullName))
            {
                return;
            }

            // 同一アセンブリで定義されている独自のenumのみ対象とする
            if (!AreContainingSameAssembly(baseSymbol.ContainingAssembly, propertyTypeSymbol.ContainingAssembly))
            {
                return;
            }

            associationEnums.Add(enumInfo);
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
