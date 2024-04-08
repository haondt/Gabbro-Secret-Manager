using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Gabbro_Secret_Manager.Core
{
    public class ToastResponseService(IPageRegistry pageRegistry)
    {
        public async Task Overwrite(List<(ToastSeverity Severity, string Message)> toasts, HttpContext httpContext, Action<IActionResult> setActionResult)
        {
            httpContext.Response.Headers.Clear();
            httpContext.Response.StatusCode = 200;

            var pageEntryFactory = pageRegistry.GetPageFactory("Toast");
            var pageEntry = await pageEntryFactory.Create(new ToastModel
            {
                Toasts = toasts
            });

            var result = pageEntry.CreateView();
            setActionResult(result);
        }

        public Task Overwrite(ToastSeverity severity, string message, HttpContext httpContext, Action<IActionResult> setActionResult)
        {
            return Overwrite([(severity, message)], httpContext, setActionResult);
        }

    }
}
