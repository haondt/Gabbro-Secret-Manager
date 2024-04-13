using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Gabbro_Secret_Manager.Domain.Filters
{
    public class ApiErrorFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            context.Result = new JsonResult(new
            {
                error = "internal error",
                detail = context.Exception.Message
            });
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = 500;
            base.OnException(context);
        }
    }
}
