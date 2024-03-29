using Microsoft.AspNetCore.Mvc;
using Gabbro_Secret_Manager.Core;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("partials")]
    public class PartialsController(PageRegistry pageRegistry, IOptions<IndexSettings> options, ISessionService sessionService) : BaseController(pageRegistry, options, sessionService)
    {
        [Route("{page}")]
        public Task<IActionResult> GetPartialView([FromRoute] string page)
        {
            return GetView(page);
        }
    }
}
