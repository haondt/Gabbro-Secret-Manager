using GabbroSecretManager.Domain.Secrets.Services;
using GabbroSecretManager.UI.Bulma.Components.Elements;
using GabbroSecretManager.UI.Secrets.Components;
using GabbroSecretManager.UI.Secrets.Models;
using GabbroSecretManager.UI.Shared.Controllers;
using GabbroSecretManager.UI.Shared.Services;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Secrets.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("secret")]
    [Authorize]
    public class SecretController(
        IComponentFactory componentFactory,
        IUISessionService sessionService,
        ISecretService secretService) : UIController
    {

        [HttpDelete("{id}")]
        public async Task<IResult> DeleteSecret(long id)
        {
            var result = await sessionService.GetNormalizedUsernameAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            await secretService.DeleteSecret(id, result.Value);
            return Results.NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IResult> GetEditSecretModal(long id)
        {
            var result = await sessionService.GetUserSessionDataAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            var secret = await secretService.TryGetSecretAsync(id, result.Value.NormalizedUsername, result.Value.EncryptionKey);
            if (!secret.HasValue)
            {
                Response.AsResponseData().Status(404);
                return await componentFactory.RenderComponentAsync(new Toast
                {
                    Message = "Secret not found",
                    Severity = Haondt.Web.BulmaCSS.Services.ToastSeverity.Error,
                });
            }

            return await componentFactory.RenderComponentAsync(new EditSecretModal
            {
                Secret = secret.Value,
                SecretId = id
            });
        }

        [HttpPost("{id}")]
        public async Task<IResult> EditSecret(long id, [FromForm] UpsertSecretRequest request)
        {
            var result = await sessionService.GetUserSessionDataAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            var updated = await secretService.TryUpdateSecretAsync(id, new()
            {
                Comments = request.Comments ?? "",
                Key = request.Key,
                Tags = request.Tags.Distinct().ToList(),
                Value = request.Value ?? ""
            }, result.Value.NormalizedUsername, result.Value.EncryptionKey);

            if (!updated.IsSuccessful)
            {
                Response.AsResponseData().Status(404);
                return await componentFactory.RenderComponentAsync(new Toast
                {
                    Message = "Secret not found",
                    Severity = Haondt.Web.BulmaCSS.Services.ToastSeverity.Error,
                });
            }

            Response.AsResponseData()
                .HxTrigger("refreshSearchResults");
            return await componentFactory.RenderComponentAsync(new CloseModal
            {
                CloseRefreshEncryptionKeyModal = false
            });
        }
    }
}
