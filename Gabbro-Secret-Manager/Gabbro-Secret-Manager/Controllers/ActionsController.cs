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
        ApiKeyService apiKeyService,
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

        [HttpPost("modal-validate-password")]
        public async Task<IActionResult> ValidatePassword([FromForm] string? password)
        {
            if (await VerifySession() is (false, var invalidSessionResponse))
                return Redirect(_indexSettings.AuthenticationPage);

            var session = await userService.GetSession(_sessionService.SessionToken!);
            if (!await userService.TryAuthenticateUser(session.UserKey, password ?? ""))
            {
                return await GetView("passwordReentryModal", () => new PasswordReentryModalModel
                {
                    Error = "incorrect password",
                    Text = password ?? ""
                });
            }

            Response.Headers["HX-Trigger-After-Settle"] = $"{{\"validatedpassword\":\"{password}\"}}";
            return await GetView("passwordReentryModal", () => new PasswordReentryModalModel { Text = password ?? ""});
        }

        [HttpPost("refresh-encryption-key")]
        public async Task<IActionResult> RefreshEncryptionKey([FromForm] string? password)
        {
            if (await VerifySession() is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            var session = await userService.GetSession(_sessionService.SessionToken!);
            var user = await userService.GetUser(session.UserKey);
            var (result, sessionToken, sessionExpiry, _) = await userService.TryAuthenticateUserAndGenerateSessionToken(user.Username, password ?? "");
            if (!result)
            {
                return await GetView("passwordReentryForm", () => new PasswordReentryFormModel
                {
                    Error = "incorrect password",
                    Text = password ?? ""
                });
            }

            _sessionService.Reset(sessionToken);
            var userData = await userDataService.GetUserData(session.UserKey);
            encryptionKeyService.UpsertEncryptionKey(sessionToken!, session.UserKey, password!, userData.EncryptionKeySettings);
            Response.Cookies.AddAuthentication(sessionToken, sessionExpiry);
            return await GetView(_indexSettings.HomePage);
        }

        [HttpDelete("delete-secret")]
        public async Task<IActionResult> DeleteSecret([FromForm] string name)
        {

            if (await VerifySession() is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            if (!encryptionKeyService.TryGet(_sessionService.SessionToken!, out var encryptionKey))
                return await GetView("passwordReentryForm");

            var userKey = await _sessionService.GetUserKeyAsync();
            await secretService.DeleteSecret(userKey, name);
            return Ok();
        }

        [HttpPost("upsert-secret")]
        public async Task<IActionResult> UpsertSecret(
            [FromForm] string key,
            [FromForm] string? value,
            [FromForm] string? comments,
            [FromForm] List<string> tags,
            [FromForm] string? currentKey)
        {
            comments ??= "";
            value ??= "";

            if (await VerifySession() is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            if (!encryptionKeyService.TryGet(_sessionService.SessionToken!, out var encryptionKey))
                return await GetView("passwordReentryForm");

            var userKey = await _sessionService.GetUserKeyAsync();
            async Task<Func<UpsertSecretFormModel>> upsertSecretFormModelGeneratorGenerator(string errorMessage)
            {
                var tagSuggestions = await secretService.GetAvailableTags(encryptionKey, userKey);
                return () => new UpsertSecretFormModel
                {
                    CurrentKey = currentKey,
                    Key = key,
                    Value = value,
                    TagSuggestions = tagSuggestions,
                    Tags = [.. tags],
                    Comments = comments,
                    Error = errorMessage
                };
            };

            if (string.IsNullOrWhiteSpace(key))
                return await GetView("upsertSecretForm", await upsertSecretFormModelGeneratorGenerator("Secret name cannot be empty"));

            if (!Regex.IsMatch(key, @"^[a-zA-z0-9_-]+$"))
                return await GetView("upsertSecretForm", await upsertSecretFormModelGeneratorGenerator("Secret name may only contain the characters [a-zA-Z0-9_-]+"));

            if (currentKey != null && currentKey != key && await secretService.ContainsSecret(userKey, key))
                return await GetView("upsertSecretForm", await upsertSecretFormModelGeneratorGenerator("Secret name already in use"));

            await secretService.UpsertSecret(encryptionKey, userKey, key, value, comments, [.. tags]);
            if (!string.IsNullOrWhiteSpace(currentKey) && currentKey != key)
                await secretService.DeleteSecret(userKey, currentKey);

            return await GetView(_indexSettings.HomePage);
        }

        [HttpPost("create-api-key")]
        public async Task<IActionResult> CreateApiKey([FromForm] string name)
        {
            if (await VerifySession() is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            if (!Request.Headers.TryGetValue("HX-Prompt", out var values) || values.Count != 1 || string.IsNullOrEmpty(values.Single()))
                return await GetToastView(ToastSeverity.Error, "Unable to retrieve password from HX-Prompt");
            var password = values.Single()!;

            var session = await userService.GetSession(_sessionService.SessionToken!);
            if (!await userService.TryAuthenticateUser(session.UserKey, password))
                return await GetToastView(ToastSeverity.Error, "incorrect password");
            var userData = await userDataService.GetUserData(session.UserKey);

            var encryptionKey = encryptionKeyService.CreateApiEncryptionKey(session.UserKey, password, userData.EncryptionKeySettings);
            var (token, id, apiKey) = await apiKeyService.CreateApiTokenAsync(session.UserKey, name, encryptionKey);

            return await GetView("settingsPage", async () =>
            {
                var apiKeys = await apiKeyService.GetApiKeys(session.UserKey);
                var model = new SettingsModel
                {
                    ShowNewKeyWarning = true,
                    ApiKeys = apiKeys.Select(kvp =>
                    {
                        var apiKey = new ViewApiKey
                        {
                            Id = kvp.Key,
                            Name = kvp.Value.Name,
                        };
                        if (kvp.Key == id)
                            apiKey.Value = token;

                        return apiKey;
                    }).ToList()
                };

                return model;
            });
        }
    }
}
