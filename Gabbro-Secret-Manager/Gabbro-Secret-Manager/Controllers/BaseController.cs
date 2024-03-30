﻿using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [ApiController]
    [Produces("text/html")]
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
                Response.Headers["HX-Trigger"] = $"{{\"onNavigate\":{{\"{NavigationBarModel.CurrentViewKey}\":\"{pageEntry.Page}\"}}}}";
            }
            return View(pageEntry.ViewPath, pageEntry.Model);
        }
    }
}
