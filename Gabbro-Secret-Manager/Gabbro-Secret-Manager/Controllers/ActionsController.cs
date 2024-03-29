using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;
using Gabbro_Secret_Manager.Domain.Services;
using Gabbro_Secret_Manager.Views.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("actions")]
    public class ActionsController(UserService userService, UserDataService userDataService, PageRegistry pageRegistry, IOptions<IndexSettings> indexOptions, ISessionService sessionService, EncryptionKeyService encryptionKeyService) : BaseController(pageRegistry, indexOptions, sessionService)
    {
        private readonly IndexSettings _indexSettings = indexOptions.Value;
        private readonly PageRegistry _pageRegistry = pageRegistry;
        private readonly ISessionService _sessionService = sessionService;

        [HttpPost("refresh-encryption-key")]
        public async Task<IActionResult> RefreshEncryptionKey([FromForm] string? password)
        {
            return Ok();
            //TODO
            /*
            return Redirect($"/{_indexSettings.AuthenticationPage}");
            if (!await _sessionService.IsAuthenticatedAsync())
            {
                if (!string.IsNullOrEmpty(_sessionService.SessionToken))
                    await userService.EndSession(_sessionService.SessionToken);
                return Redirect($"/{_indexSettings.AuthenticationPage}");
            }

            var session = await userService.GetSession(_sessionService.SessionToken!);
            var user = await userService.GetUser(session.UserKey);
            var (result, sessionToken, sessionExpiry, _) = await userService.TryAuthenticateUser(user.Username, password ?? "");
            if (!result)
            {
                var pageEntry = await _pageRegistry.GetPageFactory("passwordReentryForm").Create(new PasswordReentryFormModel
                {
                    Error = "Incorrect password",
                    Text = password ?? ""
                });
                return View(pageEntry.ViewPath, pageEntry.Model);
            }

            _sessionService.Reset(sessionToken);
            var userData = await userDataService.GetUserData(session.UserKey);
            encryptionKeyService.UpsertEncryptionKey(sessionToken!, session.UserKey, password!, userData.EncryptionKeySettings);

            Response.Cookies.AddAuthentication(sessionToken, sessionExpiry);
            return await this.GetPageView(_indexSettings.HomePage, _pageRegistry);
            */

        }

    }
}
