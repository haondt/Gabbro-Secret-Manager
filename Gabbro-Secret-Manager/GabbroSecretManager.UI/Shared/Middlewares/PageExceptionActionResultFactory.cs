using GabbroSecretManager.UI.Shared.Components;
using GabbroSecretManager.UI.Shared.Exceptions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Http;

namespace GabbroSecretManager.UI.Shared.Middlewares
{
    public class PageExceptionActionResultFactory(IComponentFactory componentFactory) : ISpecificExceptionActionResultFactory
    {
        public bool CanHandle(Exception exception)
        {
            return exception is PageException;
        }

        public Task<IResult> CreateAsync(Exception exception, HttpContext context)
        {
            if (exception is not PageException pageException)
                throw new ArgumentException(nameof(exception));

            var result = new Error
            {
                StatusCode = pageException.Code,
                Message = pageException.MessageArgument.HasValue ? pageException.MessageArgument.Value : pageException.Title,
                //Title = pageException.Title,
                Details = pageException.ToString()
            };

            return componentFactory.RenderComponentAsync(result);
        }
    }
}
