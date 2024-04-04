using Gabbro_Secret_Manager.Domain.Services;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gabbro_Secret_Manager.Domain
{
    public class JwtAuthenticationFilter(JweService jwtService) : ActionFilterAttribute
    {
        /*
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Authentication", out var values))
                return Task.FromResult(false);
            var result = jwtService.IsValid()
            return base.OnActionExecutionAsync(context, next);
        }
        private Task<bool> TryAuthenticateToken(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Authentication", out var values))
                return Task.FromResult(false);
            var result = jwtService.IsValid()
            return base.OnActionExecutionAsync(context, next);
        }
        */
    }
}
