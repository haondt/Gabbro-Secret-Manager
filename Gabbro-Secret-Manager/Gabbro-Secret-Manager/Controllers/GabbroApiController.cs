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
            var (existsSecret, secret) = await secretService.TryGetSecret(encryptionKey, secretKey);

            if (!existsSecret)
                return NotFound();

            Response.StatusCode = 200;
            return new JsonResult(new DumpSecret(secret!));
        }

        [HttpGet("secrets")]
        [Produces("application/json")]
        public async Task<IActionResult> ExportDataFile()
        {
            var (userKey, encryptionKey) = await apiSessionService.GetUserDataAsync();
            var secrets = await secretService.GetSecrets(encryptionKey, userKey);
            var result = new UserDataDump(secrets);
            return new OkObjectResult(result);
        }
    }
}


