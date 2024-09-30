using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibRedminePower.Extentions
{
    public static class HtmlDocumentExtentions
    {
        public static IElement QuerySelectorWithNullCheck(this IParentNode node, string selectors)
        {
            var result = node.QuerySelector(selectors);
            if (result != null)
                return result;

            throw new ApplicationException($"Failed to QuerySelector({selectors}). node.Children={node.GetChildrenSummary()} ");
        }

        public static string GetChildrenSummary(this IParentNode node)
        {
            return node.Children == null ? "[]" : string.Join(", ", node.Children.Select(c => $"[{c.InnerHtml.LimitRows(500, true)}]"));
        }
    }
}
