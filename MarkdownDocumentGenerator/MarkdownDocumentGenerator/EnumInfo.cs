using Microsoft.CodeAnalysis;

namespace MarkdownDocumentGenerator
{
    public class EnumValueInfo
    {
        private readonly DocumentationComment documentationComment;
        public IFieldSymbol Symbol { get; }

        public EnumValueInfo(IFieldSymbol enumValueSymbol)
        {
            Symbol = enumValueSymbol;

            var docComment = enumValueSymbol.GetDocumentationCommentXml() ?? "";
            documentationComment = new DocumentationComment(docComment);
            Value = (int)(Symbol.ConstantValue ?? 0);
        }

        public string DisplayName => Symbol.Name;
        public string Summary => documentationComment.GetSummary();
        public string Remarks => documentationComment.GetRemarks();
        public int Value { get; }
    }

    public class EnumInfo
    {
        private readonly DocumentationComment documentationComment;

        public INamedTypeSymbol Symbol { get; }

        public EnumInfo(INamedTypeSymbol enumSymbol)
        {
            Symbol = enumSymbol;

            var docComment = enumSymbol.GetDocumentationCommentXml() ?? "";
            documentationComment = new DocumentationComment(docComment);

            ParseValues();
        }

        public string DisplayName => Symbol.Name;

        public string Namespace => Symbol.ContainingNamespace?.ToString() ?? "";

        public string Summary => documentationComment.GetSummary();

        public string Remarks => documentationComment.GetRemarks();

        public string FullName => string.IsNullOrEmpty(Namespace) ? DisplayName : $"{Namespace}.{DisplayName}";

        public IReadOnlyList<EnumValueInfo> Values => values;
        private readonly List<EnumValueInfo> values = [];

        public void ParseValues()
        {
            foreach (var member in Symbol.GetMembers())
            {
                if (member is not IFieldSymbol fieldSymbol)
                {
                    continue;
                }

                if (fieldSymbol.HasConstantValue)
                {
                    var enumValueInfo = new EnumValueInfo(fieldSymbol);
                    values.Add(enumValueInfo);
                }
            }
        }
    }
}
