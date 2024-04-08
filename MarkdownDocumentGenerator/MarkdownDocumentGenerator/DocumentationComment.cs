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

            // seeタグは中身のcref, hrefをテキストとして書き起こす
            var seeElements = tagEelement.GetElementsByTagName("see");
            var seealsoElements = tagEelement.GetElementsByTagName("seealso");

            var replaceSeeElements = seeElements.Concat(seealsoElements);

            foreach (var seeElement in replaceSeeElements)
            {
                var newElement = document.CreateElement("span");
                newElement.TextContent = seeElement.GetAttribute("cref") ?? seeElement.GetAttribute("href") ?? "";

                // なんか直接seeElementを置き換えてもだめだったのでインサートしている
                tagEelement.InsertBefore(newElement, seeElement);
            }

            var elementValue = tagEelement.TextContent.Trim();

            var trimLines = elementValue
                .Split('\n')
                .Select(x => x.Trim());

            return string.Join('\n', trimLines);
        }
    }
}
