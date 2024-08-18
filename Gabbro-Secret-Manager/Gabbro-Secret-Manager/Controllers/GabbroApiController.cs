using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Filters;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

            return new OkObjectResult(DumpSecret.Create(secret!));
        }

        [HttpGet("secrets")]
        public async Task<IActionResult> SearchSecrets([FromQuery] string? name, [FromQuery] List<string> tags)
        {
            var (userKey, encryptionKey) = await apiSessionService.GetUserDataAsync();
            var secrets = await secretService.GetSecrets(encryptionKey, userKey, name, tags);
            return new OkObjectResult(secrets.Select(s => DumpSecret.Create(s)).ToList());
        }

        [HttpGet("export-data")]
        public async Task<IActionResult> ExportDataFile()
        {
            var (userKey, encryptionKey) = await apiSessionService.GetUserDataAsync();
            var secrets = await secretService.GetSecrets(encryptionKey, userKey);
            var result = UserDataDump.Create(secrets);
            return new OkObjectResult(result);
        }

        [HttpPost("import-data")]
        public async Task<IActionResult> ImportDataFile([FromForm] IFormFile file)
        {
            var (userKey, encryptionKey) = await apiSessionService.GetUserDataAsync();

            if (file.ContentType != "application/json")
                return new BadRequestObjectResult($"Expected an 'application/json' content type, but received '{file.ContentType}'");

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var contents = await reader.ReadToEndAsync();
            var secrets = JsonConvert.DeserializeObject<UserDataDump>(contents);

            if (secrets == null)
                return new BadRequestResult();


            var result = new
            {
                success = new List<string>(),
                failed = new List<string>(),
            };
            foreach (var secret in secrets.Secrets)
            {
                try
                {
                    await secretService.UpsertSecret(encryptionKey, userKey, secret.Key, secret.Value, secret.Comments, secret.Tags);
                    result.success.Add(secret.Key);
                }
                catch
                {
                    result.failed.Add(secret.Key);
                }
            }

            return new OkObjectResult(result);
        }
    }
}


