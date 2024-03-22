using Gabbro_Secret_Manager.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("favicon.ico")]
    [ApiController]
    public class FaviconController(AssetProvider assetProvider) : Controller
    {
        public IActionResult Get()
        {
            if (!assetProvider.TryGetAsset("favicon.ico", out var content))
                return NotFound();
            return File(content, "image/x-icon");
        }
    }
}
