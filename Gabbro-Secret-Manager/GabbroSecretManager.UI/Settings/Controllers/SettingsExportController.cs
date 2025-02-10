using GabbroSecretManager.Domain.Api.Models;
using GabbroSecretManager.Domain.Authentication.Services;
using GabbroSecretManager.Domain.Secrets.Models;
using GabbroSecretManager.Domain.Secrets.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GabbroSecretManager.UI.Settings.Controllers
{
    [Route("settings")]
    [Authorize]
    [ApiController]
    public class SettingsExportController(
        ISessionService internalSessionService,
        ISingleUseCacheService<(string NormalizedUsername, ExternalSecretList Secrets)> singleUseCache
        ) : Controller
    {

        [HttpGet("manage-data/retrieve-export")]
        public async Task<IResult> RetrieveExportedData(
            [FromQuery(Name = "export-id"), Required] string exportId)
        {
            var result = singleUseCache.TryPeekObject(exportId);
            if (!result.HasValue)
                return Results.NotFound();

            var normalizedUsername = await internalSessionService.GetNormalizedUsernameAsync();
            if (!normalizedUsername.HasValue)
                return Results.NotFound();

            if (result.Value.NormalizedUsername != normalizedUsername.Value)
                return Results.NotFound();

            singleUseCache.ConsumeObject(exportId);

            return Results.File(
                fileContents: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(result.Value.Secrets, ApiConstants.SerializerSettings)),
                contentType: "application/json",
                fileDownloadName: "gsm.json"
            );
        }
    }
}
