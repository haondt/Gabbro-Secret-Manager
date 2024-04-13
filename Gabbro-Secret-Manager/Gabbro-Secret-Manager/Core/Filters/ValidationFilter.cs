using Microsoft.AspNetCore.Mvc.Filters;

namespace Gabbro_Secret_Manager.Core.Filters
{
    public class ValidationFilter(ToastResponseService toaster) : ActionFilterAttribute
    {
        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                return toaster.Overwrite(
                    errors.Select(e => (ToastSeverity.Error, e)).ToList(),
                    context.HttpContext,
                    r => context.Result = r);
            }

            return base.OnActionExecutionAsync(context, next);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
                throw new InvalidOperationException();
            base.OnActionExecuting(context);
        }
    }
}
