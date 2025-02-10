using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Shared.Controllers
{
    [Route("/")]
    public class DefaultPathController
    {
        public IResult Get()
        {
            return Results.Redirect("/home");
        }
    }
}
