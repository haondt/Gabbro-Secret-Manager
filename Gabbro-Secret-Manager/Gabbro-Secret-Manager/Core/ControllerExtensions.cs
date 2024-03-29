
using Microsoft.AspNetCore.Mvc;

namespace Gabbro_Secret_Manager.Core
{
    public static class ControllerExtensions
    {
        public static async Task<IActionResult> GetPageView(this Controller controller, string page, PageRegistry pageRegistry)
        {
            var pageEntry = await pageRegistry.GetPageFactory(page).Create(controller.Request.AsRequestData());
            return controller.View(pageEntry.ViewPath, pageEntry.Model);
        }
    }
}
