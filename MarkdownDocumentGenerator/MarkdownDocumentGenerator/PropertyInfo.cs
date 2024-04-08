using Microsoft.CodeAnalysis;

namespace MarkdownDocumentGenerator
{
    public class PropertyInfo
    {
        private readonly DocumentationComment documentationComment;

        public IPropertySymbol Symbol { get; init; }

        public PropertyInfo(IPropertySymbol propertySymbol)
        {
            this.Symbol = propertySymbol;

            var propertyDocumentationCommentXml = propertySymbol.GetDocumentationCommentXml() ?? "";

            documentationComment = new DocumentationComment(propertyDocumentationCommentXml);
        }

        public string DisplayName => Symbol.Name;

        public string DisplayTypeName => GetTypeName();

        public string Summary => documentationComment.GetSummary();
        public string Remarks => documentationComment.GetRemarks();

        private string GetTypeName()
        {
            if (IsListType())
            {
                var firstTypeArgument = ((INamedTypeSymbol)Symbol.Type).TypeArguments.First();
                var typeName = firstTypeArgument.Name;

                return $"List<{typeName}>";
            }

            if (Symbol.Type.Kind == SymbolKind.ArrayType
                && Symbol.Type is IArrayTypeSymbol arrayTypeSymbol)
            {
                return $"{arrayTypeSymbol.ElementType.Name}[]";
            }

            return Symbol.Type.Name;
        }

        private bool IsListType()
        {
            return Symbol.Type.OriginalDefinition.Equals(GlobalCache.ListTypeSymbol, SymbolEqualityComparer.Default)
                   && Symbol.Type is INamedTypeSymbol namedType
                   && namedType.IsGenericType
                   && namedType.TypeArguments.Length == 1;
        }
    }
}
