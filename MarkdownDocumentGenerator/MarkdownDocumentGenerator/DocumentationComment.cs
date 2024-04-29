using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.CodeAnalysis;
using Spectre.Console;

namespace MarkdownDocumentGenerator
{
    public class DocumentationComment
    {
        private readonly IHtmlDocument? document;

        public DocumentationComment(string xml)
        {
            try
            {
                var parser = new HtmlParser();
                document = parser.ParseDocument(xml);
            }
            catch
            {
            }
        }

        public string GetSummary()
        {
            return GetTagText("summary");
        }

        public string GetRemarks()
        {
            return GetTagText("remarks");
        }

        private string GetTagText(string tag)
        {
            if (document is null)
            {
                return "";
            }

            var tagEelement = document.QuerySelector(tag);

            if (tagEelement is null)
            {
                return "";
            }

            var tempTagElement = (IElement)tagEelement.Clone(true);

            // seeタグは中身のcref, hrefをテキストとして書き起こす
            var seeTags = tempTagElement.QuerySelectorAll("see");
            var seealsoTags = tempTagElement.QuerySelectorAll("seealso");

            var replaceTags = seeTags.Concat(seealsoTags);

            foreach (var seeTag in replaceTags)
            {
                var seeTagValue = seeTag.GetAttribute("cref") ?? seeTag.GetAttribute("href") ?? "";

                if (!string.IsNullOrEmpty(seeTagValue))
                {
                    seeTagValue = seeTagValue.TrimStart("T:");

                    var spanElement = document.CreateElement("span");
                    spanElement.TextContent = $"<c>{seeTagValue}</c>";
                    seeTag.InsertBefore(spanElement);
                }
            }

            var elementValue = tempTagElement.TextContent.Trim();

            var trimLines = elementValue
                .Split('\n')
                .Select(x => x.Trim());

            return string.Join("\n", trimLines);
        }
    }
}
