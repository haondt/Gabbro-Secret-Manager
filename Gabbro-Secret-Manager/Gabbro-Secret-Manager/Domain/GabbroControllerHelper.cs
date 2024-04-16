using Gabbro_Secret_Manager.Controllers;
using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.DynamicFormFactories;
using Gabbro_Secret_Manager.Core.Views;
using Gabbro_Secret_Manager.Domain.DynamicFormFactories;
using Gabbro_Secret_Manager.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Domain
{
    public class GabbroControllerHelper(IGabbroPageRegistry pageRegistry,
        IPageRegistry nonGabbroPageRegistry,
        IOptions<IndexSettings> options,
        ISessionService sessionService,
        UserService userService,
        EncryptionKeyService encryptionKeyService) : ControllerHelper(nonGabbroPageRegistry, options, sessionService, userService), IGabbroControllerHelper
    {
        private readonly IndexSettings _indexSettings = options.Value;
        private readonly IGabbroPageRegistry _pageRegistry = pageRegistry;
        private readonly ISessionService _sessionService = sessionService;
        private readonly EncryptionKeyService _encryptionKeyService = encryptionKeyService;

        public async Task<ViewResult> GetRefreshEncryptionKeyView(BaseController controller, string? error = null)
        {
            return (await GetRefreshEncryptionKeyPageEntry(error)).CreateView(controller);
        }

        private Task<PageEntry> GetRefreshEncryptionKeyPageEntry(string? error = null)
        {
            return _pageRegistry.GetPageFactory("dynamicForm").Create(new RefreshEncryptionKeyDynamicFormFactory(error).Create());
        }

        public async Task<IActionResult> GetRefreshEncryptionKeyModalView(BaseController controller)
        {
            return await GetModal(controller, await GetRefreshEncryptionKeyPageEntry());
        }
        public async Task<(bool IsValid, IActionResult? InvalidSessionResponse, byte[] encryptionKey)> VerifyEncryptedSession(BaseController controller)
        {
            if (await VerifySession(controller) is (false, var invalidSessionResponse))
                return (false, invalidSessionResponse, []);

            if (!_encryptionKeyService.TryGetEncryptionKey(_sessionService.SessionToken!, out var encryptionKey))
                return (false, await GetRefreshEncryptionKeyModalView(controller), []);

            return (true, null, encryptionKey);
        }

        public override async Task<IActionResult> GetView(BaseController controller, string page, Func<Task<IPageModel>>? modelFactory = null, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {
            if (!_pageRegistry.TryGetPageFactory(page, out var pageEntryFactory))
            {
                pageEntryFactory = _pageRegistry.GetPageFactory(_indexSettings.HomePage);
                modelFactory = null;
                responseOptions = null;
            }

            if (pageEntryFactory.RequiresAuthentication)
                if (!await _sessionService.IsAuthenticatedAsync())
                    return await GetForceLoginView(controller);

            if (pageEntryFactory.RequiresEncryptionKey)
                if (!_encryptionKeyService.ContainsEncryptionKeyFor(_sessionService.SessionToken!))
                    return await GetRefreshEncryptionKeyModalView(controller);

            var pageEntry = modelFactory != null
                ? await pageEntryFactory.Create(await modelFactory(), responseOptions)
                : await pageEntryFactory.Create(controller.Request.AsRequestData(), responseOptions);

            return pageEntry.CreateView(controller);
        }

    }
}
