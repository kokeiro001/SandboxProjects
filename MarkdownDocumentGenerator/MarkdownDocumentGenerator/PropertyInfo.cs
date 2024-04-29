﻿using Microsoft.CodeAnalysis;

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

                if (IsListType(typeSymbol))
                {
                    var firstTypeArgument = ((INamedTypeSymbol)typeSymbol).TypeArguments.First();

                    var originalTypeName = getTypeName(firstTypeArgument);

                    if (isNullable)
                    {
                        return $"List<{originalTypeName}>?";
                    }
                    else
                    {
                        return $"List<{originalTypeName}>";
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

                return GetOriginalTypeName(typeSymbol);
            }

            var typeName = getTypeName(Symbol.Type);

            return typeName;
        }

        private static string GetOriginalTypeName(ITypeSymbol typeSymbol)
        {
            var isNullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

            if (isNullable)
            {
                var namedTypeSymbol = (INamedTypeSymbol)typeSymbol;

                var firstTypeArgument = namedTypeSymbol.TypeArguments.FirstOrDefault();

                if (firstTypeArgument == null)
                {
                    return $"{namedTypeSymbol.Name}?";
                }
                else
                {
                    return $"{firstTypeArgument.Name}?";
                }
            }

            return typeSymbol.Name;
        }

        private static bool IsListType(ITypeSymbol typeSymbol)
        {
            return typeSymbol.OriginalDefinition.Equals(GlobalCache.ListTypeSymbol, SymbolEqualityComparer.Default)
                   && typeSymbol is INamedTypeSymbol namedType
                   && namedType.IsGenericType
                   && namedType.TypeArguments.Length == 1;
        }
    }
}
