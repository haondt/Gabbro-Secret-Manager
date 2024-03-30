using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;
using Gabbro_Secret_Manager.Domain.Services;
using Gabbro_Secret_Manager.Views.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("actions")]
    public class ActionsController(
        UserService userService,
        UserDataService userDataService,
        PageRegistry pageRegistry,
        IOptions<IndexSettings> indexOptions,
        ISessionService sessionService,
        EncryptionKeyService encryptionKeyService,
        SecretService secretService) : BaseController(pageRegistry, indexOptions, sessionService)
    {
        private readonly IndexSettings _indexSettings = indexOptions.Value;
        private readonly PageRegistry _pageRegistry = pageRegistry;
        private readonly ISessionService _sessionService = sessionService;

        private async Task<(bool IsValid, IActionResult? InvalidSessionResponse)> VerifySession()
        {
            if (!await _sessionService.IsAuthenticatedAsync())
            {
                if (!string.IsNullOrEmpty(_sessionService.SessionToken))
                    await userService.EndSession(_sessionService.SessionToken);
                return (false, await GetView(_indexSettings.AuthenticationPage));
            }
            return (true, null);
        }

        [HttpPost("refresh-encryption-key")]
        public async Task<IActionResult> RefreshEncryptionKey([FromForm] string? password)
        {
            if (await VerifySession() is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            var session = await userService.GetSession(_sessionService.SessionToken!);
            var user = await userService.GetUser(session.UserKey);
            var (result, sessionToken, sessionExpiry, _) = await userService.TryAuthenticateUser(user.Username, password ?? "");
            if (!result)
            {
                return await GetView("passwordReentryForm", () => new PasswordReentryFormModel
                {
                    Error = "Incorrect password",
                    Text = password ?? ""
                });
            }

            _sessionService.Reset(sessionToken);
            var userData = await userDataService.GetUserData(session.UserKey);
            encryptionKeyService.UpsertEncryptionKey(sessionToken!, session.UserKey, password!, userData.EncryptionKeySettings);
            Response.Cookies.AddAuthentication(sessionToken, sessionExpiry);
            return await GetView(_indexSettings.HomePage);
        }

        [HttpPost("upsert-secret")]
        public async Task<IActionResult> UpsertSecret(
            [FromForm] bool shouldOverwriteExisting,
            [FromForm] string key,
            [FromForm] string value,
            [FromForm] string comments)
        {
            if (await VerifySession() is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            if (!encryptionKeyService.TryGet(_sessionService.SessionToken!, out var encryptionKey))
                return await GetView("passwordReentryForm");

            Func<UpsertSecretFormModel> upsertSecretFormModelGeneratorGenerator(string errorMessage) => () => new UpsertSecretFormModel
            {
                ShouldOverwriteExisting = shouldOverwriteExisting,
                Key = key,
                Value = value,
                Comments = comments,
                Error = errorMessage
            };

            if (string.IsNullOrWhiteSpace(key))
                return await GetView("upsertSecretForm", upsertSecretFormModelGeneratorGenerator("Secret name cannot be empty"));

            if (!Regex.IsMatch(key, @"^[a-zA-z0-9_-]+$"))
                return await GetView("upsertSecretForm", upsertSecretFormModelGeneratorGenerator("Secret name may only contain the characters [a-zA-Z0-9_-]+"));

            var userKey = await _sessionService.GetUserKeyAsync();
            if (!shouldOverwriteExisting && await secretService.ContainsSecret(userKey, key))
                return await GetView("upsertSecretForm", upsertSecretFormModelGeneratorGenerator("Secret name already in use"));

            await secretService.UpsertSecret(encryptionKey, userKey, key, value);
            return await GetView(_indexSettings.HomePage);
        }
    }
}
