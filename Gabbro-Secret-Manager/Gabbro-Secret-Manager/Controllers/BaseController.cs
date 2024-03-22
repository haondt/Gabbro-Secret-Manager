﻿using Gabbro_Secret_Manager.Core;
using Microsoft.AspNetCore.Mvc;

namespace Gabbro_Secret_Manager.Controllers
{
    [ApiController]
    [Produces("text/html")]
    public class BaseController : Controller
    {
        protected ViewResult View(IViewModel model)
        {
            return View(model.ViewPath, model);
        }
    }
}
