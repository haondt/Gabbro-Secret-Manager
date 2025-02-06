using GabbroSecretManager.Domain.Authentication.Services;
using GabbroSecretManager.Domain.Cryptography.Services;
using GabbroSecretManager.UI.Authentication.Components;
using GabbroSecretManager.UI.Bulma.Components.Elements;
using Haondt.Core.Models;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Shared.Services
{
    internal class UISessionService(ISessionService sessionService,
        IComponentFactory componentFactory,
        IEncryptionKeyCacheService keyCacheService) : IUISessionService
    {
        public async Task<Result<(string NormalizedUsername, byte[] EncryptionKey), IResult>> GetUserSessionDataAsync(ControllerBase controller)
        {
            var normalizedUsername = await sessionService.GetNormalizedUsernameAsync();
            if (!normalizedUsername.HasValue)
            {
                controller.Response.AsResponseData()
                    .Status(401)
                    .Header("Hx-Redirect", "/authentication/login");
                return new(await componentFactory.RenderComponentAsync<CloseModal>());
            }

            var encryptionKey = keyCacheService.TryGetEncryptionKey(normalizedUsername.Value);
            if (!encryptionKey.HasValue)
            {
                controller.Response.AsResponseData()
                    .Status(401)
                    .HxReswap("none");
                return new(await componentFactory.RenderComponentAsync<RefreshEncryptionKeyModal>());
            }

            return new((normalizedUsername.Value, encryptionKey.Value));
        }
    }
}
