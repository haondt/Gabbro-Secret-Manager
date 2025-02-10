using GabbroSecretManager.Domain.Shared.Exceptions;
using GabbroSecretManager.UI.Bulma.Components.Elements;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GabbroSecretManager.UI.Shared.Middlewares
{
    public class ToastExceptionActionResultFactory(IComponentFactory componentFactory, ILogger<ToastExceptionActionResultFactory> logger) : ISpecificExceptionActionResultFactory
    {
        public bool CanHandle(Exception exception)
        {
            return true;
        }

        public async Task<IResult> CreateAsync(Exception exception, HttpContext context)
        {
            logger.LogError(exception, "Toasting exception {Exception}", exception.Message);
            var severity = ToastSeverity.Error;
            var statusCode = 500;
            var model = new Toast { Message = $"{exception.GetType().Name}: {exception.Message}", Severity = severity };
            if (exception is UserException)
            {
                model.Message = exception.Message;
                model.Severity = ToastSeverity.Warning;
                statusCode = 400;
            }

            var errorComponent = await componentFactory.RenderComponentAsync(model);
            context.Response.AsResponseData()
                .Status(statusCode)
                .HxReswap("none");
            return errorComponent;
        }
    }
}
