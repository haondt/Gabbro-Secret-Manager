using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("components")]
    public class ComponentsController(
        IOptions<IndexSettings> options,
        PageRegistry pageRegistry,
        ISessionService sessionService) : BaseController(pageRegistry, options, sessionService)
    {
        PageRegistry _pageRegistry = pageRegistry;

        [HttpGet("toast")]
        public Task<IActionResult> GetToast([FromQuery] ToastSeverity severity, [FromQuery] string message) => GetToastView(severity, message);

        [HttpGet("modal")]
        public async Task<IActionResult> GetModal([FromQuery] string content, [FromQuery] string? source)
        {
            var modal = await _pageRegistry.GetPageFactory("modal").Create(new ModalModel
            {
                Source = source,
                Content = await _pageRegistry.GetPageFactory(content).Create(Request.AsRequestData())
            });

            Response.Headers.Clear();
            Response.Headers.Append("HX-Reswap", "afterbegin");
            Response.Headers.Append("HX-Retarget", "body");

            return View(modal.ViewPath, modal.Model);
        }
    }
}
