using GabbroSecretManager.Api.Filters;
using GabbroSecretManager.Api.Models;
using GabbroSecretManager.Api.Services;
using GabbroSecretManager.Domain.Secrets.Services;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.Api.Controllers
{
    [ApiController]
    [Route("api")]
    [ApiValidationFilter]
    [ServiceFilter(typeof(ApiAuthenticationFilter))]
    [ApiErrorFilter]
    [Produces("application/json")]
    public class GabbroSecretManagerApiController(IApiSessionService sessionService,
        ISecretService secretService)
    {
        [HttpGet("secret/{id}")]
        public async Task<IActionResult> GetSecret([FromRoute] long id)
        {
            var (normalizedUsername, encryptionKey) = await sessionService.GetUserDataAsync();
            var secret = await secretService.TryGetSecretAsync(id, normalizedUsername, encryptionKey);

            if (!secret.HasValue)
                return new NotFoundResult();

            return new OkObjectResult(ApiSecret.FromSecret(secret.Value));
        }

        [HttpGet("secrets")]
        public async Task<IActionResult> SearchSecrets([FromQuery] string? name, [FromQuery] List<string> tags)
        {
            var (normalizedUsername, encryptionKey) = await sessionService.GetUserDataAsync();
            var secrets = await secretService.SearchSecrets(
                normalizedUsername,
                encryptionKey,
                name ?? "",
                tags.Count > 0 ? [.. tags] : null);

            return new OkObjectResult(secrets.Select(s => ApiSecret.FromSecret(s.Secret)).ToList());
        }

        [HttpGet("export-data")]
        public async Task<IActionResult> ExportDataFile()
        {
            var (normalizedUsername, encryptionKey) = await sessionService.GetUserDataAsync();
            var secrets = await secretService.GetSecrets(normalizedUsername, encryptionKey);
            var result = new ApiSecretList
            {
                Secrets = secrets.Select(q => ApiSecret.FromSecret(q.Secret)).ToList()
            };
            return new OkObjectResult(result);
        }
    }
}

