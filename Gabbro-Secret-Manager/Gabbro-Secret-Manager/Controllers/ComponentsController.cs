using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("components")]
    public class ComponentsController(IControllerHelper helper) : BaseController
    {
        [HttpGet("toast")]
        public Task<IActionResult> GetToast([FromQuery] ToastSeverity severity, [FromQuery] string message) => helper.GetToastView(this, severity, message);
    }
}
