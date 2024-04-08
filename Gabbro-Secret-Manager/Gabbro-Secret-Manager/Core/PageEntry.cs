
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Gabbro_Secret_Manager.Core
{
    public class PageEntry
    {
        public required string Page { private get; init; }
        public required string ViewPath { private get; init; }
        public required object Model { private get; init; }
        public Action<IHeaderDictionary>? ConfigureResponse { private get; init; }

        public ViewResult CreateView(Controller controller)
        {
            ConfigureResponse?.Invoke(controller.Response.Headers);
            return controller.View(ViewPath, Model);
        }

        public ViewResult CreateView(IHeaderDictionary response)
        {
            ConfigureResponse?.Invoke(response);

            var vdd = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = Model
            };

            return new ViewResult
            {
                ViewName = ViewPath,
                ViewData = vdd
            };
        }

        public Task<IHtmlContent> PartialAsync(IHtmlHelper html)
        {
            return html.PartialAsync(ViewPath, Model);
        }
    }
}