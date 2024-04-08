using Gabbro_Secret_Manager.Controllers;
using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.DynamicFormFactories;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Core
{
    public class ControllerHelper(IPageRegistry pageRegistry,
        IOptions<IndexSettings> options,
        ISessionService sessionService,
        UserService userService) : IControllerHelper
    {
        private readonly IndexSettings _indexSettings = options.Value;

        public async Task<IActionResult> GetModal(BaseController controller, PageEntry content, bool allowClickOut = false)
        {
            var modal = await pageRegistry.GetPageFactory("modal").Create(new ModalModel
            {
                Content = content,
                AllowClickOut = allowClickOut
            });
            return modal.CreateView(controller);
        }

        public Task<IActionResult> GetModal(BaseController controller, IDynamicFormFactory content, bool allowClickOut = false) => GetModal(controller, "dynamicForm", content, allowClickOut);

        public async Task<IActionResult> GetModal(BaseController controller, string page, IDynamicFormFactory content, bool allowClickOut = false)
        {
            var modal = await pageRegistry.GetPageFactory("modal").Create(new ModalModel
            {
                Content = await pageRegistry.GetPageFactory(page).Create(content.Create()),
                AllowClickOut = allowClickOut
            });
            return modal.CreateView(controller);
        }

        public async Task<IActionResult> GetToastView(BaseController controller, List<(ToastSeverity Severity, string Message)> toasts)
        {
            controller.Response.StatusCode = 200;

            var pageEntryFactory = pageRegistry.GetPageFactory("toast");
            var pageEntry = await pageEntryFactory.Create(new ToastModel
            {
                Toasts = toasts
            });

            return pageEntry.CreateView(controller);
        }

        public Task<IActionResult> GetToastView(BaseController controller, ToastSeverity severity, string message) => GetToastView(controller, [(severity, message)]);

        public async Task<IActionResult> GetForceLoginView(BaseController controller)
        {
            var content = await pageRegistry.GetPageFactory("dynamicForm").Create(new AlertDynamicFormFactory(

                "access denied",
                "please log in to continue",
                "partials/login"
                ).Create());

            return await GetModal(controller, content, false);
        }

        public async Task<(bool IsValid, IActionResult? InvalidSessionResponse)> VerifySession(BaseController controller)
        {
            if (!await sessionService.IsAuthenticatedAsync())
            {
                if (!string.IsNullOrEmpty(sessionService.SessionToken))
                    await userService.EndSession(sessionService.SessionToken);
                return (false, await GetForceLoginView(controller));
            }
            return (true, null);
        }


        public virtual async Task<IActionResult> GetView(BaseController controller, string page, Func<Task<IPageModel>>? modelFactory = null, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {
            if (!pageRegistry.TryGetPageFactory(page, out var pageEntryFactory))
            {
                pageEntryFactory = pageRegistry.GetPageFactory(_indexSettings.HomePage);
                modelFactory = null;
                responseOptions = null;
            }

            if (pageEntryFactory.RequiresAuthentication)
                if (!await sessionService.IsAuthenticatedAsync())
                    return await GetForceLoginView(controller);

            var pageEntry = modelFactory != null
                ? await pageEntryFactory.Create(await modelFactory(), responseOptions)
                : await pageEntryFactory.Create(controller.Request.AsRequestData(), responseOptions);

            return pageEntry.CreateView(controller);
        }

        public Task<IActionResult> GetView(BaseController controller, string page, IDynamicFormFactory dynamicFormFactory, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
            => GetView(controller, page, () => Task.FromResult<IPageModel>(dynamicFormFactory.Create()), responseOptions);
        public Task<IActionResult> GetView(BaseController controller, IDynamicFormFactory dynamicFormFactory, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
            => GetView(controller, "dynamicForm", () => Task.FromResult<IPageModel>(dynamicFormFactory.Create()), responseOptions);

        public Task<IActionResult> GetView(BaseController controller, string page, Func<IPageModel> modelFactory, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
            => GetView(controller, page, () => Task.FromResult(modelFactory()), responseOptions);
    }
}
