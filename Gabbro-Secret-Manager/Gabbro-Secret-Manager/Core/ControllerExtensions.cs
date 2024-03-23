
using Microsoft.AspNetCore.Mvc;

namespace Gabbro_Secret_Manager.Core
{
    public static class ControllerExtensions
    {
        public static IActionResult GetPartialPageView(this Controller controller, string page, PageRegistry pageRegistry)
        {
            var pageEntry = pageRegistry.GetPartialPage(page).Create(controller.Request.AsRequestData());
            return controller.View(pageEntry.ViewPath, pageEntry.Model);
        }
        public static IActionResult GetPageView(this Controller controller, string page, PageRegistry pageRegistry)
        {
            var pageEntry = pageRegistry.GetPage(page).Create(controller.Request.AsRequestData());
            return controller.View(pageEntry.ViewPath, pageEntry.Model);
        }
    }
}
