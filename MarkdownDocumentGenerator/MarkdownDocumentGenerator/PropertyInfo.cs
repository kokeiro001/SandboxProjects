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

        override public string ToString()
        {
            return $"{DisplayTypeName} {DisplayName}";
        }

        public string DisplayName => Symbol.Name;

        public string DisplayTypeName => GetTypeName();

        public string Summary => documentationComment.GetSummary();
        public string Remarks => documentationComment.GetRemarks();

        private string GetTypeName()
        {
            static string getTypeName(ITypeSymbol typeSymbol)
            {
                var isNullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

                if (typeSymbol is INamedTypeSymbol namedTypeSymbol
                    && namedTypeSymbol.IsGenericType)
                {
                    // 値型は基本型がNullableになるため分岐する
                    if (namedTypeSymbol.IsValueType)
                    {
                        var firstTypeArgument = namedTypeSymbol.TypeArguments.First();
                        return $"{getTypeName(firstTypeArgument)}?";
                    }
                    else
                    {
                        var genericOriginalTypeName = namedTypeSymbol.Name;

                        var typeNames = namedTypeSymbol.TypeArguments
                            .Select(x => getTypeName(x))
                            .ToArray();

                        var joinedTypeNames = string.Join(", ", typeNames);

                        if (isNullable)
                        {
                            return $"{genericOriginalTypeName}<{joinedTypeNames}>?";
                        }
                        else
                        {
                            return $"{genericOriginalTypeName}<{joinedTypeNames}>";
                        }
                    }
                }

                if (typeSymbol.Kind == SymbolKind.ArrayType
                    && typeSymbol is IArrayTypeSymbol arrayTypeSymbol)
                {
                    var originalTypeName = getTypeName(arrayTypeSymbol.ElementType);

                    if (isNullable)
                    {
                        return $"{originalTypeName}[]?";
                    }
                    else
                    {
                        return $"{originalTypeName}[]";
                    }
                }


                var typeName = GetAliasTypeNameIfExists(typeSymbol);

                if (isNullable)
                {
                    return $"{typeName}?";
                }
                else
                {
                    return typeName;
                }
            }

            var typeName = getTypeName(Symbol.Type);

            return typeName;
        }

        private static string GetAliasTypeNameIfExists(ITypeSymbol typeSymbol)
        {
            var typeName = typeSymbol.Name switch
            {
                "String" => "string",
                "Int32" => "int",
                _ => typeSymbol.Name,
            };

            return typeName;
        }
    }
}
