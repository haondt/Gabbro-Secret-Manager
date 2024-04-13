using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Filters;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gabbro_Secret_Manager.Controllers
{
    [ApiController]
    [Route("api")]
    [ApiValidationFilter]
    [ServiceFilter(typeof(ApiAuthenticationFilter))]
    [ApiErrorFilter]
    [Produces("application/json")]
    public class GabbroApiController(SecretService secretService, ApiSessionService apiSessionService) : ControllerBase
    {
        [HttpGet("secrets/{secretName}")]
        public async Task<IActionResult> GetSecret([FromRoute] string secretName)
        {
            var (userKey, encryptionKey) = await apiSessionService.GetUserDataAsync();
            var secretKey = Secret.GetStorageKey(userKey, secretName);
            var (existsSecret, secret, comments, secretTags) = await secretService.TryGetSecret(encryptionKey, secretKey);

            if (!existsSecret)
                return NotFound();

            Response.StatusCode = 200;
            return new JsonResult(new
            {
                Name = secretName,
                Value = secret,
                Comments = comments,
                Tags = secretTags
            });
        }
    }
}


