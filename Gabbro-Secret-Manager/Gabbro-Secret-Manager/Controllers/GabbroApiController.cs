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
        [HttpGet("secret/{id}")]
        public async Task<IActionResult> GetSecret([FromRoute] Guid id)
        {
            var (userKey, encryptionKey) = await apiSessionService.GetUserDataAsync();
            var secretKey = Secret.GetStorageKey(userKey, id);
            var (existsSecret, secret) = await secretService.TryGetSecret(encryptionKey, secretKey);

            if (!existsSecret)
                return NotFound();

            return new OkObjectResult(new DumpSecret(secret!));
        }

        [HttpGet("secrets")]
        public async Task<IActionResult> SearchSecrets([FromQuery] string? secretName, [FromQuery] List<string> tags)
        {
            var (userKey, encryptionKey) = await apiSessionService.GetUserDataAsync();
            var secrets = await secretService.GetSecrets(encryptionKey, userKey, secretName, tags);
            return new OkObjectResult(secrets.Select(s => new DumpSecret(s)).ToList());
        }

        [HttpGet("export-data")]
        public async Task<IActionResult> ExportDataFile()
        {
            var (userKey, encryptionKey) = await apiSessionService.GetUserDataAsync();
            var secrets = await secretService.GetSecrets(encryptionKey, userKey);
            var result = new UserDataDump(secrets);
            return new OkObjectResult(result);
        }
    }
}


