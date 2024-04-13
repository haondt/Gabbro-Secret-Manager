using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gabbro_Secret_Manager.Domain.Filters
{
    public class ApiAuthenticationFilter(ApiSessionService apiSessionService) : ActionFilterAttribute
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
