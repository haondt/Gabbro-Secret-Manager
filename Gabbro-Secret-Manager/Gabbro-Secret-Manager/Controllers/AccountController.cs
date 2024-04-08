using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.DynamicForms;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("account")]
    public class AccountController(
        IControllerHelper helper,
        UserService userService, IPageRegistry pageRegistry, IOptions<IndexSettings> indexOptions, ISessionService sessionService, LifetimeHookService lifetimeHookService) : BaseController
    {
        private readonly IPageRegistry _pageRegistry = pageRegistry;
        private readonly IndexSettings _indexSettings = indexOptions.Value;
        private readonly ISessionService _sessionService = sessionService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string? username, [FromForm] string? password)
        {
            username ??= "";
            password ??= "";
            var (result, sessionToken, sessionExpiry ,userKey) = await userService.TryAuthenticateUserAndGenerateSessionToken(username, password);
            if (!result)
                return await helper.GetView(this, "login", new LoginDynamicFormFactory(username, "incorrect username or password"));

            _sessionService.Reset(sessionToken);
            await lifetimeHookService.OnLoginAsync(username, password, userKey, sessionToken);
            Response.Cookies.AddAuthentication(sessionToken, sessionExpiry);
            return await helper.GetView(this, _indexSettings.HomePage);
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            if (await _sessionService.IsAuthenticatedAsync())
            {
                var sessionToken = _sessionService.SessionToken;
                await userService.EndSession(sessionToken!);
                Response.Cookies.ExpireAuthentication();
            }

            return await helper.GetView(this, _indexSettings.AuthenticationPage);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string? username, [FromForm] string? password)
        {
            username ??= "";
            password ??= "";
            var (result, usernameReason, passwordReason, user, userKey) = await userService.TryRegisterUser(username, password);
            if (!result)
                return await helper.GetView(this, "register", () => new RegisterModel
                {
                    Username = username,
                    UsernameError = usernameReason,
                    PasswordError = passwordReason
                });

            await lifetimeHookService.OnRegisterAsync(user!, userKey);
            return await helper.GetView(this, _indexSettings.AuthenticationPage);
        }
    }
}
