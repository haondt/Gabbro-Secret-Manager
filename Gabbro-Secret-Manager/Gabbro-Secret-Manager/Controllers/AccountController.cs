using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Route("account")]
    public class AccountController(UserService userService, PageRegistry pageRegistry, IOptions<IndexSettings> indexOptions, ISessionService sessionService, LifetimeHookService lifetimeHookService) : BaseController(pageRegistry, indexOptions, sessionService)
    {
        private readonly PageRegistry _pageRegistry = pageRegistry;
        private readonly IndexSettings _indexSettings = indexOptions.Value;
        private readonly ISessionService _sessionService = sessionService;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string? username, [FromForm] string? password)
        {
            username ??= "";
            password ??= "";
            var (result, sessionToken, sessionExpiry ,userKey) = await userService.TryAuthenticateUser(username, password);
            if (!result)
                return await GetView("login", () => new LoginModel
                {
                    Username = username,
                    Password = password,
                    Error = "Incorrect username or password",
                });

            _sessionService.Reset(sessionToken);
            await lifetimeHookService.OnLoginAsync(username, password, userKey, sessionToken);
            Response.Cookies.AddAuthentication(sessionToken, sessionExpiry);
            return await GetView(_indexSettings.HomePage);
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

            return await GetView(_indexSettings.HomePage);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] string? username, [FromForm] string? password)
        {
            username ??= "";
            password ??= "";
            var (result, usernameReason, passwordReason, user, userKey) = await userService.TryRegisterUser(username, password);
            if (!result)
                return await GetView("register", () => new RegisterModel
                {
                    Username = username,
                    Password = password,
                    UsernameError = usernameReason,
                    PasswordError = passwordReason
                });

            await lifetimeHookService.OnRegisterAsync(user!, userKey);
            return await this.GetPageView(_indexSettings.AuthenticationPage, _pageRegistry);
        }
    }
}
