using Microsoft.AspNetCore.Mvc;
using Gabbro_Secret_Manager.Core;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("partials")]
    public class PartialsController(IControllerHelper helper) : BaseController
    {
        [Route("{page}")]
        public Task<IActionResult> GetPartialView([FromRoute] string page)
        {
            return helper.GetView(this, page);
        }
    }
}
