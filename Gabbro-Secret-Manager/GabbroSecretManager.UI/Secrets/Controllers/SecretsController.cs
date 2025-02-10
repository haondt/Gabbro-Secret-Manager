using GabbroSecretManager.Domain.Secrets.Services;
using GabbroSecretManager.UI.Bulma.Components.Elements;
using GabbroSecretManager.UI.Home.Components;
using GabbroSecretManager.UI.Secrets.Components;
using GabbroSecretManager.UI.Secrets.Models;
using GabbroSecretManager.UI.Shared.Controllers;
using GabbroSecretManager.UI.Shared.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Secrets.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("secrets")]
    [Authorize]
    public class SecretsController(IComponentFactory componentFactory,
        IUISessionService sessionService,
        ISecretService secretService
        ) : UIController
    {
        [HttpGet("create")]
        public Task<IResult> GetCreateModal()
        {
            return componentFactory.RenderComponentAsync<CreateSecretModal>();
        }

        [HttpGet]
        public async Task<IResult> GetSecrets(
            [FromQuery(Name = "swap-tags")] bool swapTags)
        {
            var result = await sessionService.GetUserSessionDataAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            var secrets = await secretService.GetSecrets(
                result.Value.NormalizedUsername,
                result.Value.EncryptionKey);

            var secretList = new SecretList
            {
                Secrets = secrets
            };

            Microsoft.AspNetCore.Components.IComponent component = swapTags
                ? new AppendComponentLayout
                {
                    Components = new()
                    {
                        secretList,
                        new TagsFilter
                        {
                            Swap = true,
                            Tags = secrets.SelectMany(s => s.Secret.Tags).Distinct().ToList()
                        }
                    }
                }
                : secretList;

            return await componentFactory.RenderComponentAsync(component);
        }

        [HttpGet("search")]
        public async Task<IResult> SearchSecrets(
            [FromQuery] List<string> tags,
            [FromQuery] string? search)
        {
            var result = await sessionService.GetUserSessionDataAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            var secrets = tags.Count > 0
                    ? await secretService.SearchSecrets(
                        result.Value.NormalizedUsername,
                        result.Value.EncryptionKey,
                        search ?? "",
                        tags.ToHashSet())
                    : await secretService.SearchSecrets(
                        result.Value.NormalizedUsername,
                        result.Value.EncryptionKey,
                        search ?? "");

            var secretList = new SecretList
            {
                Secrets = secrets,
                Swap = true
            };

            return await componentFactory.RenderComponentAsync(secretList);
        }


        [HttpPost("create")]
        public async Task<IResult> CreateSecret([FromForm] UpsertSecretRequest request)
        {

            var result = await sessionService.GetUserSessionDataAsync(this);
            if (!result.IsSuccessful)
                return result.Reason;

            await secretService.CreateSecret(new()
            {
                Comments = request.Comments ?? "",
                Key = request.Key,
                Tags = request.Tags.Distinct().ToList(),
                Value = request.Value ?? "",
            }, result.Value.NormalizedUsername, result.Value.EncryptionKey);

            Response.AsResponseData()
                .HxTrigger("refreshSearchResults");
            return await componentFactory.RenderComponentAsync(new CloseModal
            {
                CloseRefreshEncryptionKeyModal = false
            });
        }
    }
}
