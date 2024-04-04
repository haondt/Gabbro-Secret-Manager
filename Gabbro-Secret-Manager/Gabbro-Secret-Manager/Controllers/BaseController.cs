using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Produces("text/html")]
    [ServiceFilter(typeof(ToastErrorFilter))]
    [ServiceFilter(typeof(ValidationFilter))]
    public class BaseController(PageRegistry pageRegistry, IOptions<IndexSettings> options, ISessionService sessionService) : Controller
    {
        protected Task<IActionResult> GetView(string page, Func<IPageModel> modelFactory) => GetView(page, () => Task.FromResult(modelFactory()));
        protected async Task<IActionResult> GetView(string page, Func<Task<IPageModel>>? modelFactory = null)
        {
            var usingGivenPage = true;
            if (!pageRegistry.TryGetPageFactory(page, out var pageEntryFactory))
            {
                usingGivenPage = false;
                if (!pageRegistry.TryGetPageFactory(options.Value.HomePage, out pageEntryFactory))
                    throw new InvalidOperationException("No home page registered");
            }

            if (pageEntryFactory.RequiresAuthentication)
                if (!await sessionService.IsAuthenticatedAsync())
                {
                    usingGivenPage = false;
                    if (!pageRegistry.TryGetPageFactory(options.Value.AuthenticationPage, out pageEntryFactory))
                        throw new InvalidOperationException("No authentication page registered");
                }

            PageEntry pageEntry;
            if (usingGivenPage)
                if (modelFactory != null)
                    pageEntry = await pageEntryFactory.Create(await modelFactory());
                else
                    pageEntry = await pageEntryFactory.Create(Request.AsRequestData());
            else
                pageEntry = await pageEntryFactory.Create(Request.AsRequestData());

            if (!string.IsNullOrEmpty(pageEntry.SetUrl))
            {
                Response.Headers["HX-Push-Url"] = pageEntry.SetUrl;
                Response.Headers["HX-Trigger-After-Settle"] = $"{{\"on_navigate\":{{\"{NavigationBarModel.CurrentViewKey}\":\"{pageEntry.Page}\"}}}}";
            }
            return View(pageEntry.ViewPath, pageEntry.Model);
        }
        protected async Task<IActionResult> GetToast(List<(ToastSeverity Severity, string Message)> toasts)
        {
            Response.Headers.Clear();
            Response.Headers.Append("HX-Reswap", "afterbegin");
            Response.Headers.Append("HX-Retarget", "#toast-container");
            Response.StatusCode = 200;

            var pageEntryFactory = pageRegistry.GetPageFactory("Toast");
            var pageEntry = await pageEntryFactory.Create(new ToastModel
            {
                Toasts = toasts
            });

            return View(pageEntry.ViewPath, pageEntry.Model);
        }

        protected Task<IActionResult> GetToastView(ToastSeverity severity, string message) => GetToast([(severity, message)]);
    }
}
