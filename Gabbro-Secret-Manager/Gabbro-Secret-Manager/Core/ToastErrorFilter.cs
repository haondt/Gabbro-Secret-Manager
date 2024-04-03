using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net;

namespace Gabbro_Secret_Manager.Core
{
    public class ToastErrorFilter(ToastResponseService toaster) : ExceptionFilterAttribute
    {
        public override async Task OnExceptionAsync(ExceptionContext context)
        {

            var error = context.Exception;
            var severity = ToastSeverity.Error;
            if (error is ToastableException te)
            {
                severity = te.Severity;
                error = te.Inner;
            }

            await toaster.Overwrite(
                severity,
                error.Message,
                context.HttpContext,
                r => context.Result = r);

            await base.OnExceptionAsync(context);
        }
    }
}
