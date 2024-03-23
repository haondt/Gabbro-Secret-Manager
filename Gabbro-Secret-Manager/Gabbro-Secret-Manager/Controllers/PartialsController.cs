using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("partials")]
    public class PartialsController(PageRegistry pageRegistry, UserService userService) : BaseController
    {
        [HttpGet("{page}")]
        public async Task<IActionResult> GetView([FromRoute] string page)
        {
            if (!pageRegistry.TryGetPartialPage(page, out var pageRegistryEntry)
                 || pageRegistryEntry!.Type != PageEntryType.Partial)
                return NotFound();

            if (pageRegistryEntry.RequiresAuthentication)
            {
                if (await Request.AsRequestData().IsAuthenticated(userService) is not (true, _))
                    return NotFound();
            }

            var pageEntry = pageRegistryEntry.Create(Request.AsRequestData());
            return View(pageEntry.ViewPath, pageEntry.Model);
        }
    }
}
