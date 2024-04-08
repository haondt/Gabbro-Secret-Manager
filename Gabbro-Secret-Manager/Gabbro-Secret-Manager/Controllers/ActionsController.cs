using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.Views;
using Gabbro_Secret_Manager.Domain;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Services;
using Gabbro_Secret_Manager.Views.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("actions")]
    public class ActionsController(
        IGabbroControllerHelper helper,
        UserService userService,
        UserDataService userDataService,
        IPageRegistry pageRegistry,
        IOptions<IndexSettings> indexOptions,
        ApiKeyService apiKeyService,
        ISessionService sessionService,
        EncryptionKeyService encryptionKeyService,
        SecretService secretService) : BaseController
    {
        private readonly IndexSettings _indexSettings = indexOptions.Value;
        private readonly IPageRegistry _pageRegistry = pageRegistry;
        private readonly ISessionService _sessionService = sessionService;


        [HttpPost("refresh-encryption-key")]
        public async Task<IActionResult> RefreshEncryptionKey([FromForm] string? password)
        {
            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            var session = await userService.GetSession(_sessionService.SessionToken!);
            var user = await userService.GetUser(session.UserKey);
            var (result, sessionToken, sessionExpiry, _) = await userService.TryAuthenticateUserAndGenerateSessionToken(user.Username, password ?? "");
            if (!result)
                return await helper.GetRefreshEncryptionKeyView(this, "incorrect password");

            _sessionService.Reset(sessionToken);
            var userData = await userDataService.GetUserData(session.UserKey);
            encryptionKeyService.UpsertEncryptionKey(sessionToken!, session.UserKey, password!, userData.EncryptionKeySettings);
            Response.Cookies.AddAuthentication(sessionToken, sessionExpiry);
            return await helper.GetView(this, _indexSettings.HomePage);
        }

        [HttpDelete("delete-secret")]
        public async Task<IActionResult> DeleteSecret([FromForm] string name)
        {

            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

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

            var (isSessionValid, invalidSessionResponse, encryptionKey) = await helper.VerifyEncryptedSession(this);
            if (!isSessionValid)
                return invalidSessionResponse!;

            var userKey = await _sessionService.GetUserKeyAsync();
            async Task<Func<UpsertSecretFormModel>> upsertSecretFormModelGeneratorGenerator(string errorMessage)
            {
                var tagSuggestions = await secretService.GetAvailableTags(encryptionKey!, userKey);
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
                return await helper.GetView(this, "upsertSecretForm", await upsertSecretFormModelGeneratorGenerator("Secret name cannot be empty"));

            if (!Regex.IsMatch(key, @"^[a-zA-z0-9_-]+$"))
                return await helper.GetView(this, "upsertSecretForm", await upsertSecretFormModelGeneratorGenerator("Secret name may only contain the characters [a-zA-Z0-9_-]+"));

            if (currentKey != null && currentKey != key && await secretService.ContainsSecret(userKey, key))
                return await helper.GetView(this, "upsertSecretForm", await upsertSecretFormModelGeneratorGenerator("Secret name already in use"));

            await secretService.UpsertSecret(encryptionKey, userKey, key, value, comments, [.. tags]);
            if (!string.IsNullOrWhiteSpace(currentKey) && currentKey != key)
                await secretService.DeleteSecret(userKey, currentKey);

            return await helper.GetView(this, _indexSettings.HomePage);
        }

        // TODO
        private Task<PageEntry> GetApiKeyPasswordConfirmationForm(string name, string? error = null)
        {
            return _pageRegistry.GetPageFactory("dynamicForm").Create(new DynamicFormModel
            {
                Title = "please confirm password to continue",
                HxPost = "actions/create-api-key/complete",
                HxVals = $"{{\"name\":\"{name}\"}}",
                Items =
                [
                    new DynamicFormInput
                    {
                        Name = "password",
                        Type = DynamicFormInputType.Password,
                        Autocomplete = "current-password",
                        Error = error
                    }
                ],
                Buttons = 
                [
                    new DynamicFormButton
                    {
                           Text = "submit",
                           Type = DynamicFormButtonType.Submit
                    },
                    new DynamicFormButton
                    {
                           Text = "cancel",
                           Type = DynamicFormButtonType.Button,
                           HyperTrigger = "closeModal"
                    }
                ],
            });
        }

        [HttpGet("export-data")]
        public async Task<IActionResult> ExportData()
        {
            var (isSessionValid, invalidSessionResponse, _) = await helper.VerifyEncryptedSession(this);
            if (!isSessionValid)
                return invalidSessionResponse!;

            var content = await _pageRegistry.GetPageFactory("confirmExportDataPrompt").Create(Request.AsRequestData());
            return await helper.GetModal(this, content, true);
        }

        [HttpGet("export-data/secrets.json")]
        [Produces("text/json")]
        public async Task<IActionResult> ExportDataFile()
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

        [HttpPost("create-api-key/init")]
        public async Task<IActionResult> CreateApiKey([FromForm] string? name)
        {
            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            if (string.IsNullOrEmpty(name))
            {
                return await helper.GetView(this, "settingsPage", () => new SettingsModel
                {
                    NameError = "Name cannot be empty"
                }, 
                o => o
                    .ReSelect("#settings .form-error.for-name")
                    .ReTarget("#settings .form-error.for-name")
                    .ReSwap("outerHTML"));
            }

            return await helper.GetModal(this, await GetApiKeyPasswordConfirmationForm(name), false);
        }

        [HttpPost("create-api-key/complete")]
        public async Task<IActionResult> CreateApiKey([FromForm] string name, [FromForm] string password)
        {
            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            var session = await userService.GetSession(_sessionService.SessionToken!);
            if (!await userService.TryAuthenticateUser(session.UserKey, password))
                return await helper.GetModal(this, await GetApiKeyPasswordConfirmationForm(name, "incorrect password"), false);
            var userData = await userDataService.GetUserData(session.UserKey);

            var encryptionKey = encryptionKeyService.CreateApiEncryptionKey(session.UserKey, password, userData.EncryptionKeySettings);
            var (token, id, apiKey) = await apiKeyService.CreateApiTokenAsync(session.UserKey, name, encryptionKey);

            return await helper.GetView(this, "settingsPage", async () =>
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
