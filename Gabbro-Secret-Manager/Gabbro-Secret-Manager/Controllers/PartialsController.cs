using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("partials")]
    public class PartialsController(PageRegistry pageRegistry) : BaseController
    {
        [HttpGet("{page}")]
        public IActionResult GetView([FromRoute] string page)
        {
            if (pageRegistry.TryGetPage(page, out var pageRegistryEntry)
                && pageRegistryEntry!.Type == PageEntryType.Partial)
                return View(pageRegistryEntry!.Create(Request.RouteValues));
            return NotFound();
        }
    }
}
