using GabbroSecretManager.Domain.Api.Services;
using GabbroSecretManager.Domain.Authentication.Services;
using GabbroSecretManager.Domain.Cryptography.Services;
using GabbroSecretManager.Domain.Secrets.Models;
using GabbroSecretManager.Domain.Secrets.Services;
using GabbroSecretManager.UI.Bulma.Components.Elements;
using GabbroSecretManager.UI.Settings.Components;
using GabbroSecretManager.UI.Shared.Controllers;
using GabbroSecretManager.UI.Shared.Extensions;
using GabbroSecretManager.UI.Shared.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace GabbroSecretManager.UI.Settings.Controllers
{
    [Route("settings")]
    [Authorize]
    public class SettingsController(
        IUISessionService sessionService,
        IEncryptionKeyCacheService keyCacheService,
        ISingleUseCacheService<(string NormalizedUsername, ExternalSecretList Secrets)> singleUseCache,
        ISecretService secretService,
        IApiKeyService apiKeyService,
        IComponentFactory componentFactory,
        IUserService userService) : UIController
    {
        [HttpGet]
        public async Task<IResult> Get()
        {
            var result = await sessionService.GetNormalizedUsernameAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            return await componentFactory.RenderComponentAsync(new Settings.Components.Settings
            {
                NormalizedUsername = result.Value
            });
        }

        [HttpDelete("api-key/{id}")]
        public async Task<IResult> DeleteKey(string id)
        {
            var result = await sessionService.GetNormalizedUsernameAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            await apiKeyService.DeleteApiKey(result.Value, id);
            return Results.NoContent();
        }

        [HttpPost("api-keys/create")]
        public Task<IResult> CreateApiKey([FromForm(Name = "name"), Required] string _)
        {
            return componentFactory.RenderComponentAsync(new EnrichWithPasswordModal
            {
                Payload = Request.Form.Select(kvp =>
                    new KeyValuePair<string, string>(kvp.Key, kvp.Value.LastOrDefault() ?? "")).ToDictionary(),
                Target = "/settings/api-keys/enriched-create"
            });
        }

        [HttpPost("api-keys/enriched-create")]
        public async Task<IResult> CreateApiKey(
            [FromForm(Name = "name"), Required] string name,
            [FromForm(Name = "password"), Required] string password)
        {
            var result = await userService.TryValidatePassword(password);
            if (result.IsSuccessful)
            {
                var encryptionKey = keyCacheService.GenerateEncryptionKey(result.Value.NormalizedUsername, password, result.Value.EncryptionKeySettings);
                var (token, apiKey) = await apiKeyService.CreateApiKey(name, result.Value.NormalizedUsername, password, encryptionKey);
                return await componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = new()
                    {
                        new CloseModal(),
                        new ManageApiKeysBody()
                        {
                            Swap = true,
                            NormalizedUsername = result.Value.NormalizedUsername,
                            NewApiKey = (apiKey.Id, apiKey.Name, token)
                        }
                    }
                });

            }

            if (result.Reason.FailedToGetUsername)
                return await sessionService.AuthenticationSignOut(this);

            var error = result.Reason.IncorrectPassword
                ? "Password incorrect."
                : "Unable to authenticate with the provided password.";

            Response.AsResponseData()
                .HxTrigger("updateErrors", error, "#enrich-with-password-errors");
            return Results.Unauthorized();
        }

        [HttpPost("manage-data/import")]
        public async Task<IResult> ImportData([FromForm, Required] IFormFile file)
        {
            var parsedData = file.DeserializeFromJson<ExternalSecretList>();
            var result = await sessionService.GetUserSessionDataAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            await secretService.ImportSecrets(parsedData, result.Value.NormalizedUsername, result.Value.EncryptionKey);
            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = new()
                {
                    new CloseModal(),
                    new Toast
                    {
                        Message = "Secrets imported.",
                        Severity = Haondt.Web.BulmaCSS.Services.ToastSeverity.Success
                    }
                }
            });
        }

        [HttpGet("manage-data/export")]
        public Task<IResult> ExportData()
        {
            return componentFactory.RenderComponentAsync(new EnrichWithPasswordModal
            {
                Payload = [],
                Target = "/settings/manage-data/enriched-export"
            });
        }

        [HttpPost("manage-data/enriched-export")]
        public async Task<IResult> ExportData(
            [FromForm(Name = "password"), Required] string password)
        {
            var result = await userService.TryValidatePassword(password);
            if (result.IsSuccessful)
            {
                var encryptionKey = keyCacheService.GenerateEncryptionKey(result.Value.NormalizedUsername, password, result.Value.EncryptionKeySettings);
                var exportedData = await secretService.ExportSecrets(result.Value.NormalizedUsername, encryptionKey);
                var exportId = Guid.NewGuid().ToString();
                singleUseCache.CacheObject(exportId, (result.Value.NormalizedUsername, exportedData));

                Response.AsResponseData()
                    .HxTrigger("click", target: ".modal-background")
                    .Header("Hx-Redirect", $"/settings/manage-data/retrieve-export?export-id={exportId}");

                return await componentFactory.RenderComponentAsync<CloseModal>();
            }

            if (result.Reason.FailedToGetUsername)
                return await sessionService.AuthenticationSignOut(this);

            var error = result.Reason.IncorrectPassword
                ? "Password incorrect."
                : "Unable to authenticate with the provided password.";

            Response.AsResponseData()
                .HxTrigger("updateErrors", error, "#enrich-with-password-errors");
            return Results.Unauthorized();
        }

        [HttpPost("manage-data/delete-all-secrets")]
        public Task<IResult> DeleteAllSecrets()
        {
            return componentFactory.RenderComponentAsync(new EnrichWithPasswordModal
            {
                Payload = [],
                Target = "/settings/manage-data/enriched-delete-all-secrets"
            });
        }

        [HttpPost("manage-data/enriched-delete-all-secrets")]
        public async Task<IResult> DeleteAllSecrets(
            [FromForm(Name = "password"), Required] string password)
        {
            var result = await userService.TryValidatePassword(password);
            if (result.IsSuccessful)
            {
                await secretService.DeleteAllSecrets(result.Value.NormalizedUsername);
                return await componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = new()
                    {
                        new CloseModal(),
                        new Toast{
                            Message = "All secrets deleted.",
                            Severity = Haondt.Web.BulmaCSS.Services.ToastSeverity.Success
                        }
                    }
                });
            }

            if (result.Reason.FailedToGetUsername)
                return await sessionService.AuthenticationSignOut(this);

            var error = result.Reason.IncorrectPassword
                ? "Password incorrect."
                : "Unable to authenticate with the provided password.";

            Response.AsResponseData()
                .HxTrigger("updateErrors", error, "#enrich-with-password-errors");
            return Results.Unauthorized();
        }
    }
}
