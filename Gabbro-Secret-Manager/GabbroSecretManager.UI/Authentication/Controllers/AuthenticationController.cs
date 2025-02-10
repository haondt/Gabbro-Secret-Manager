using GabbroSecretManager.Domain.Authentication.Services;
using GabbroSecretManager.UI.Authentication.Components;
using GabbroSecretManager.UI.Bulma.Components.Elements;
using GabbroSecretManager.UI.Shared.Components;
using GabbroSecretManager.UI.Shared.Controllers;
using GabbroSecretManager.UI.Shared.Services;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Components;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Authentication.Controllers
{
    [Route("authentication")]
    public class AuthenticationController(
        IUISessionService sessionService,
        IComponentFactory componentFactory, IUserService userService) : UIController
    {
        [HttpGet("login")]
        public Task<IResult> GetLogin()
        {
            return componentFactory.RenderComponentAsync<Login>();
        }

        [HttpGet("register")]
        public Task<IResult> GetRegister()
        {
            return componentFactory.RenderComponentAsync<Register>();
        }

        [HttpPost("register")]
        public async Task<IResult> Register([FromForm] string username, [FromForm] string password, [FromForm(Name = "confirm-password")] string confirmPassword)
        {
            if (confirmPassword != password)
            {
                return await componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = new()
                    {
                        new Toast
                        {
                            Message = "Failed to register account",
                            Severity = ToastSeverity.Error
                        },
                        new UpdateFormErrors
                        {
                            Errors =["Passwords must match."]
                        }
                    }
                });
            }
            var result = await userService.TryRegister(username, password);
            if (result.Success)
            {
                Response.AsResponseData().HxPushUrl("/authentication/login");
                return await componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = new()
                    {
                        new Toast
                        {
                            Message = "Registered successfully",
                            Severity = ToastSeverity.Success
                        },
                        new HxSwapOob
                        {
                            Content = new Login(),
                            Target = "#page-container"
                        }
                    }

                });
            }
            else
            {
                return await componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = new()
                    {
                        new Toast
                        {
                            Message = "Failed to register account",
                            Severity = ToastSeverity.Error
                        },
                        new UpdateFormErrors
                        {
                            Errors = result.Errors,
                        }
                    }
                });
            }
        }

        [HttpPost("login")]
        public async Task<IResult> Login([FromForm] string username, [FromForm] string password)
        {
            var result = await userService.TrySignIn(username, password);

            if (!result.Success)
            {
                return await componentFactory.RenderComponentAsync(new AppendComponentLayout
                {
                    Components = new()
                    {
                        new Toast
                        {
                            Message = "Failed to log in",
                            Severity = ToastSeverity.Error
                        },
                        new UpdateFormErrors
                        {
                            Errors = result.Errors
                        }
                    }

                });
            }

            Response.AsResponseData().Header("HX-Redirect", "/home");
            return Results.Ok();
        }

        [HttpPost("sign-out")]
        public Task<IResult> AuthenticationSignOut()
        {
            return sessionService.AuthenticationSignOut(this);
        }

        [HttpGet("refresh")]
        public Task<IResult> GetRefreshEncryptionKeyModal()
        {
            return componentFactory.RenderComponentAsync<RefreshEncryptionKeyModal>();
        }

        [HttpPost("refresh")]
        public async Task<IResult> RefreshEncryptionKey([FromForm] string password)
        {
            var result = await userService.TryRefreshEncryptionKey(password);
            if (result.Success)
            {
                Response.AsResponseData()
                    .HxTrigger("encryptionKeyRefreshed");
                return await componentFactory.RenderComponentAsync(new CloseModal
                {
                    CloseMainModal = false
                });
            }

            if (result.FailedToGetUsername)
                return await sessionService.AuthenticationSignOut(this);

            var error = result.IncorrectPassword
                ? "Password incorrect."
                : "Unable to authenticate with the provided password.";

            Response.AsResponseData()
                .HxTrigger("updateErrors", error, "#refresh-encryption-key-errors");
            return Results.Unauthorized();
        }
    }
}
