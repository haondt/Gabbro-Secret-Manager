using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.Extensions.Primitives;
using Gabbro_Secret_Manager.Domain;

namespace Gabbro_Secret_Manager.Controllers
{
    public class IndexController(
        IOptions<IndexSettings> options,
        PageRegistry pageRegistry,
        UserService userService) : BaseController
    {

        [Route("/")]
        public Task<IActionResult> Redirect()
        {
            return Get(null);
        }

        [Route("{page}")]
        public async Task<IActionResult> Get([FromRoute] string? page)
        {
            var existingPage = page ?? options.Value.HomePage;
            if (!pageRegistry.TryGetPage(existingPage, out var pageEntry)
                || pageEntry!.Type != PageEntryType.Page)
                if (!pageRegistry.TryGetPage(options.Value.HomePage, out pageEntry)
                    || pageEntry!.Type != PageEntryType.Page)
                    throw new InvalidOperationException("No home page registered");

            if (pageEntry.RequiresAuthentication)
            {
                if (await Request.AsRequestData().IsAuthenticated(userService) is not (true, _))
                    if (!pageRegistry.TryGetPage(options.Value.AuthenticationPage, out pageEntry)
                        || pageEntry!.Type != PageEntryType.Page)
                    throw new InvalidOperationException("No authentication page registered");
            }

            return View("~/Core/Views/Index.cshtml",
                new IndexModel
                {
                    NavigationBar = pageRegistry.GetPartialPage("navigationBar").Create(new NavigationBarModel
                    {
                        CurrentPage = existingPage,
                        Pages = options.Value.NavigationBarPages
                    }),
                    Title = options.Value.SiteName,
                    Content = pageEntry!.Create(Request.AsRequestData())
                });
        }
    }
}
