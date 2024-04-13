using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.Filters;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Controllers
{
    [Produces("text/html")]
    [ServiceFilter(typeof(ToastErrorFilter))]
    [ServiceFilter(typeof(ValidationFilter))]
    public class BaseController : Controller { }
}
