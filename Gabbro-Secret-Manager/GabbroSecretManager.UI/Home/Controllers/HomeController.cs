using GabbroSecretManager.UI.Shared.Controllers;
using Haondt.Web.Core.Extensions;
using Haondt.Web.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Home.Controllers
{
    [Route("home")]
    public class HomeController(IComponentFactory componentFactory) : UIController
    {
        [HttpGet]
        [Authorize]
        public Task<IResult> Get()
        {
            return componentFactory.RenderComponentAsync<Home.Components.Home>();
        }
    }
}
