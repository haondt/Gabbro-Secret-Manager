using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gabbro_Secret_Manager.Controllers
{
    [ApiController]
    [Route("api")]
    public class GabbroApiController(ISessionService sessionService, 
        EncryptionKeyService encryptionKeyService,
        SecretService secretService)
    {
        [HttpGet("export-data/secrets.json")]
        public async Task<IActionResult> GetData()
        {
            if (!await sessionService.IsAuthenticatedAsync())
                return new UnauthorizedResult();

            if (!encryptionKeyService.TryGet(sessionService.SessionToken!, out var encryptionKey))
                return new UnauthorizedResult();

            var userKey = await sessionService.GetUserKeyAsync();
            var secrets = await secretService.GetSecrets(encryptionKey, userKey);

            var result = new UserDataDump
            {
                Secrets = secrets.Select(s => new DumpSecret
                {
                    Key = s.Key,
                    Value = s.Value,
                    Tags = s.Tags
                }).ToList()
            };

            return new OkObjectResult(result);
        }

    }
}
