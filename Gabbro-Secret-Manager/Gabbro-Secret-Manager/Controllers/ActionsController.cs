using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.Views;
using Gabbro_Secret_Manager.Domain;
using Gabbro_Secret_Manager.Domain.DynamicFormFactories;
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
        SecretService secretService,
        AuthenticationService authenticationService) : BaseController
    {
        private readonly IndexSettings _indexSettings = indexOptions.Value;


        [HttpPost("refresh-encryption-key")]
        public async Task<IActionResult> RefreshEncryptionKey([FromForm] string? password)
        {
            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            var session = await userService.GetSession(sessionService.SessionToken!);
            var user = await userService.GetUser(session.Owner);
            var (result, sessionToken, sessionExpiry, _) = await userService.TryAuthenticateUserAndGenerateSessionToken(user.Username, password ?? "");
            if (!result)
                return await helper.GetRefreshEncryptionKeyView(this, "incorrect password");

            sessionService.Reset(sessionToken);
            var userData = await userDataService.GetUserData(session.Owner);
            encryptionKeyService.CreateEncryptionKey(sessionToken, session.Owner, password!, userData.EncryptionKeySettings);
            authenticationService.AddAuthentication(sessionToken, sessionExpiry);
            return await helper.GetView(this, _indexSettings.HomePage);
        }

        [HttpDelete("delete-secret")]
        public async Task<IActionResult> DeleteSecret([FromForm] Guid id)
        {

            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            var userKey = await sessionService.GetUserKeyAsync();
            await secretService.DeleteSecret(userKey, id);
            return Ok();
        }

        [HttpPost("upsert-secret")]
        public async Task<IActionResult> UpsertSecret(
            [FromForm] Guid? id,
            [FromForm] string name,
            [FromForm] string? value,
            [FromForm] string? comments,
            [FromForm] List<string> tags)
        {
            comments ??= "";
            value ??= "";

            var (isSessionValid, invalidSessionResponse, encryptionKey) = await helper.VerifyEncryptedSession(this);
            if (!isSessionValid)
                return invalidSessionResponse!;

            var userKey = await sessionService.GetUserKeyAsync();
            async Task<Func<UpsertSecretFormModel>> upsertSecretFormModelGeneratorGenerator(string errorMessage)
            {
                var tagSuggestions = await secretService.GetAvailableTags(encryptionKey!, userKey);
                return () => new UpsertSecretFormModel
                {
                    Name = name,
                    Id = id,
                    Value = value,
                    TagSuggestions = tagSuggestions,
                    Tags = [.. tags],
                    Comments = comments,
                    Error = errorMessage
                };
            };

            if (string.IsNullOrWhiteSpace(name))
                return await helper.GetView(this, "upsertSecretForm", await upsertSecretFormModelGeneratorGenerator("Secret name cannot be empty"));

            if (!Regex.IsMatch(name, @"^[a-zA-z0-9_-]+$"))
                return await helper.GetView(this, "upsertSecretForm", await upsertSecretFormModelGeneratorGenerator("Secret name may only contain the characters [a-zA-Z0-9_-]+"));

            await secretService.UpsertSecret(encryptionKey, userKey, name, value, comments, [.. tags], id);

            return await helper.GetView(this, _indexSettings.HomePage);
        }

        [HttpGet("export-data")]
        public async Task<IActionResult> ExportData()
        {
            var (isSessionValid, invalidSessionResponse, _) = await helper.VerifyEncryptedSession(this);
            if (!isSessionValid)
                return invalidSessionResponse!;

            var content = await pageRegistry.GetPageFactory("confirmExportDataPrompt").Create(Request.AsRequestData());
            return await helper.GetModal(this, content, true);
        }

        [HttpGet("export-data/secrets.json")]
        [Produces("application/json")]
        public async Task<IActionResult> ExportDataFile()
        {
            if (!await sessionService.IsAuthenticatedAsync())
                return new UnauthorizedResult();

            if (!encryptionKeyService.TryGetEncryptionKey(sessionService.SessionToken!, out var encryptionKey))
                return new UnauthorizedResult();

            var userKey = await sessionService.GetUserKeyAsync();
            var secrets = await secretService.GetSecrets(encryptionKey, userKey);
            var result = new UserDataDump(secrets);
            return new OkObjectResult(result);
        }

        [HttpPost("create-api-key")]
        public async Task<IActionResult> CreateApiKey([FromForm] string? name)
        {
            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            if (string.IsNullOrEmpty(name))
            {
                return await helper.GetView(this, "settings", () => new SettingsModel
                {
                    NameError = "Name cannot be empty"
                }, 
                o => o
                    .ReSelect("#settings .form-error.for-name")
                    .ReTarget("#settings .form-error.for-name")
                    .ReSwap("outerHTML"));
            }

            return await helper.GetModal(this, new ApiKeyPasswordConfirmationDynamicFormFactory(name), false);
        }

        [HttpPost("create-api-key/complete")]
        public async Task<IActionResult> CreateApiKey([FromForm] string name, [FromForm] string password)
        {
            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            var session = await userService.GetSession(sessionService.SessionToken!);
            if (!await userService.TryAuthenticateUser(session.Owner, password))
                return await helper.GetView(this, new ApiKeyPasswordConfirmationDynamicFormFactory(name, "incorrect password"));
            var userData = await userDataService.GetUserData(session.Owner);

            var encryptionKey = encryptionKeyService.CreateEncryptionKey(session.Owner, password, userData.EncryptionKeySettings);
            var (token, apiKey) = await apiKeyService.CreateApiTokenAsync(session.Owner, name, encryptionKey);

            return await helper.GetView(this, "settings", async () =>
            {
                var apiKeys = await apiKeyService.GetApiKeys(session.Owner);
                var model = new SettingsModel
                {
                    ShowNewKeyWarning = true,
                    ApiKeys = apiKeys
                        .OrderByDescending(kvp => kvp.Value.Created)
                        .Select(kvp =>
                        {
                            var viewApiKey = new ViewApiKey
                            {
                                Id = kvp.Value.Id.ToString(),
                                Name = kvp.Value.Name,
                            };
                            if (kvp.Value.Id == apiKey.Id)
                                viewApiKey.Value = token;
                            return viewApiKey;
                        }).ToList()
                };

                return model;
            });
        }

        [HttpPost("delete-api-key")]
        public async Task<IActionResult> DeleteApiKey([FromForm] Guid id)
        {
            if (await helper.VerifySession(this) is (false, var invalidSessionResponse))
                return invalidSessionResponse!;

            var session = await userService.GetSession(sessionService.SessionToken!);
            if (await apiKeyService.VerifyOwner(session.Owner, id))
                await apiKeyService.DeleteApiKey(id);

            return Ok();
        }
    }
}
