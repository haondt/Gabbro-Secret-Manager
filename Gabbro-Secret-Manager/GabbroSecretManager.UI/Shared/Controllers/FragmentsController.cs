using GabbroSecretManager.Domain.Shared.Exceptions;
using GabbroSecretManager.UI.Bulma.Components.Elements;
using Haondt.Web.BulmaCSS.Services;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Shared.Controllers
{
    [Route("fragments")]
    public class FragmentsController(IComponentFactory componentFactory) : UIController
    {
        [HttpGet("tag")]
        public Task<IResult> GetTag([FromQuery] string tag,
            [FromQuery(Name = "color-classes")] string? colorClasses)
        {
            if (string.IsNullOrWhiteSpace(tag))
                throw new UserException("Tag cannot be empty");
            var tagComponent = new Tag { Text = tag.Trim() };
            if (!string.IsNullOrEmpty(colorClasses))
                tagComponent.ColorClasses = colorClasses;
            return componentFactory.RenderComponentAsync(tagComponent);
        }

        [HttpGet("toast")]
        public Task<IResult> GetToast([FromQuery] string message, [FromQuery] ToastSeverity severity)
        {
            return componentFactory.RenderComponentAsync(new Toast
            {
                Message = message,
                Severity = severity
            });
        }
    }
}
