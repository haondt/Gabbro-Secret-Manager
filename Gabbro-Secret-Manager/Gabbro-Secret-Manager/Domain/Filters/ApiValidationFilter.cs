using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gabbro_Secret_Manager.Domain.Filters
{
    public class ApiValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                context.Result = new JsonResult(new
                {
                    error = "invalid request",
                    detail = errors
                });
                context.HttpContext.Response.ContentType = "application/json";
                context.HttpContext.Response.StatusCode = 400;
                
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
