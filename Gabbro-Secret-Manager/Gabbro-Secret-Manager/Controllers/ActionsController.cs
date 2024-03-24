using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain;
using Gabbro_Secret_Manager.Domain.Services;
using Gabbro_Secret_Manager.Views.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("actions")]
    public class ActionsController(UserService userService, UserDataService userDataService, PageRegistry pageRegistry, IOptions<IndexSettings> indexOptions, EncryptionKeyService encryptionKeyService) : BaseController
    {

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string? username, [FromForm] string? password)
        {
            username ??= "";
            password ??= "";
            var (result, sessionToken, sessionExpiry) = await userService.TryAuthenticateUser(username, password);
            if (result)
            {
                await encryptionKeyService.GetOrCreateEncryptionKey(sessionToken, password);

                Response.Cookies.AddAuthentication(sessionToken, sessionExpiry);
                Response.Headers["HX-Replace-Url"] = $"/{indexOptions.Value.HomePage}";
                return this.GetPartialPageView(indexOptions.Value.HomePage, pageRegistry);
            }

            var pageEntry = pageRegistry.GetPartialPage("login").Create(new LoginModel
            {
                Username = username,
                Password = password,
                Error = "Incorrect username or password",
            });

            return View(pageEntry.ViewPath, pageEntry.Model);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            if (await Request.AsRequestData().IsAuthenticated(userService) is not (true, _))
                return Ok();

            var sessionToken = Request.AsRequestData().GetAuthentication();
            await userService.EndSession(sessionToken);
            Response.Cookies.ExpireAuthentication();
            return Ok();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string? username, [FromForm] string? password)
        {
            username ??= "";
            password ??= "";
            var (result, usernameReason, passwordReason, _, userKey) = await userService.TryRegisterUser(username, password);
            if (result)
            {
                await userDataService.InitializeUserData(userKey);
                Response.Headers["HX-Replace-Url"] = $"/{indexOptions.Value.AuthenticationPage}";
                return this.GetPartialPageView(indexOptions.Value.AuthenticationPage, pageRegistry);
            }

            var pageEntry = pageRegistry.GetPartialPage("register").Create(new RegisterModel
            {
                Username = username,
                Password = password,
                UsernameError = usernameReason,
                PasswordError = passwordReason
            });

            return View(pageEntry.ViewPath, pageEntry.Model);
        }
    }
}
