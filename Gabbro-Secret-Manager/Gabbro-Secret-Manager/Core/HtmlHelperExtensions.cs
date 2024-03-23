using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gabbro_Secret_Manager.Core
{
    public static class HtmlHelperExtensions
    {
        public static Task<IHtmlContent> PartialAsync(this IHtmlHelper html, PageEntry content)
        {
            return html.PartialAsync(content.ViewPath, content.Model);
        }
   }
}
