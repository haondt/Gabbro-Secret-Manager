using Gabbro_Secret_Manager.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("assets")]
    public class AssetsController(
        AssetProvider assetProvider,
        StylesProvider stylesProvider,
        FileExtensionContentTypeProvider contentTypeProvider) : BaseController
    {

        [Route("{**assetPath}")]
        public IActionResult Get(string assetPath)
        {
            if ("style.css".Equals(assetPath))
                return  Content(stylesProvider.GetStyles(), "text/css");

            if (assetPath.Contains('/') || assetPath.Contains('\\'))
                return BadRequest("Invalid path.");;

            if (!contentTypeProvider.TryGetContentType(assetPath, out var contentType))
                return BadRequest("Unsupported file type.");

            if (!assetProvider.TryGetAsset(assetPath, out var content))
                return NotFound();

            return File(content, contentType);
        }
    }
}
