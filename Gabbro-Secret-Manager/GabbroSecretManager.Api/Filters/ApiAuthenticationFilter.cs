using GabbroSecretManager.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GabbroSecretManager.Api.Filters
{
    public class ApiAuthenticationFilter(IApiSessionService apiSessionService) : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!await apiSessionService.IsAuthenticatedAsync())
            {
                context.HttpContext.Response.StatusCode = 401;
                context.Result = new UnauthorizedResult();
                return;
            }

            await base.OnActionExecutionAsync(context, next);
        }
    }
}
