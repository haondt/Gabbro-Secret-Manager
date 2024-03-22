using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Controllers
{
    public class IndexController(IOptions<IndexSettings> options, PageRegistry pageRegistry) : BaseController
    {

        [Route("/")]
        public IActionResult Redirect()
        {
            return Get(null);
        }

        [Route("{page}")]
        public IActionResult Get([FromRoute] string? page)
        {
            var existingPage = page ?? options.Value.HomePage;
            if (!pageRegistry.TryGetPage(existingPage, out var pageEntry)
                || pageEntry!.Type != PageEntryType.Page)
                return NotFound();


            return View("~/Core/Views/Index.cshtml",
                new IndexModel
                {
                    NavigationBar = pageRegistry.GetPage("navigationBar").Create(new Dictionary<string, object?> { { "current", existingPage } }),
                    Title = options.Value.SiteName,
                    Content = pageEntry!.Create(Request.RouteValues)
                });
        }
    }
}
