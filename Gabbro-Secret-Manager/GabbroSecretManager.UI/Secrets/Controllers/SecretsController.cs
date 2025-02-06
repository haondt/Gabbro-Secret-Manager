using GabbroSecretManager.Domain.Authentication.Services;
using GabbroSecretManager.Domain.Cryptography.Services;
using GabbroSecretManager.Domain.Secrets.Services;
using GabbroSecretManager.UI.Authentication.Components;
using GabbroSecretManager.UI.Bulma.Components.Elements;
using GabbroSecretManager.UI.Home.Components;
using GabbroSecretManager.UI.Secrets.Components;
using GabbroSecretManager.UI.Secrets.Models;
using GabbroSecretManager.UI.Shared.Components;
using GabbroSecretManager.UI.Shared.Controllers;
using Haondt.Core.Models;
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
        ISessionService sessionService,
        ISecretService secretService,
        IEncryptionKeyCacheService keyCacheService) : UIController
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
            var result = await GetUserSessionData();
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
                            Tags = secrets.SelectMany(s => s.Tags).Distinct().ToList()
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
            var result = await GetUserSessionData();
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

        private async Task<Result<(string NormalizedUsername, byte[] EncryptionKey), IResult>> GetUserSessionData()
        {
            var normalizedUsername = await sessionService.GetNormalizedUsernameAsync();
            if (!normalizedUsername.HasValue)
            {
                Response.AsResponseData()
                    .Status(401)
                    .Header("Hx-Redirect", "/authentication/login");
                return new(await componentFactory.RenderComponentAsync<CloseModal>());
            }

            var encryptionKey = keyCacheService.TryGetEncryptionKey(normalizedUsername.Value);
            if (!encryptionKey.HasValue)
            {
                Response.AsResponseData()
                    .Status(401)
                    .HxReswap("none");
                return new(await componentFactory.RenderComponentAsync<RefreshEncryptionKeyModal>());
            }

            return new((normalizedUsername.Value, encryptionKey.Value));
        }

        [HttpPost("create")]
        public async Task<IResult> CreateSecret([FromForm] UpsertSecretRequest request)
        {

            var result = await GetUserSessionData();
            if (!result.IsSuccessful)
                return result.Reason;

            await secretService.CreateSecret(new()
            {
                Comments = request.Comments ?? "",
                Key = request.Key,
                Tags = request.Tags,
                Value = request.Value ?? "",
            }, result.Value.NormalizedUsername, result.Value.EncryptionKey);

            return await componentFactory.RenderComponentAsync(new AppendComponentLayout
            {
                Components = new()
                {
                    new CloseModal(),
                    new HxSwapOob
                    {
                        Content = new Home.Components.Home(),
                        Target = "#page-container"
                    }
                }
            });
        }
    }
}
