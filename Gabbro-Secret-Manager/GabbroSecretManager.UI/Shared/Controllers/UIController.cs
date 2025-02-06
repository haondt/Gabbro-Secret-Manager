using GabbroSecretManager.UI.Shared.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace GabbroSecretManager.UI.Shared.Controllers
{

    [Produces("text/html")]
    [ServiceFilter(typeof(ModelStateValidationFilter))]
    public class UIController : Controller
    {
    }
}
