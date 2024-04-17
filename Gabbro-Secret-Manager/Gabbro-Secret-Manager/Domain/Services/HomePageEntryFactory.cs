using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class HomePageEntryFactory(ISessionService sessionService, EncryptionKeyService encryptionKeyService, SecretService secretService) : IGabbroPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public string Page => "home";
        public string ViewPath => "Home";

        public bool RequiresEncryptionKey => true;

        public async Task<PageEntry> Create(IPageRegistry pageRegistry, IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {

            var encryptionKey = encryptionKeyService.GetEncryptionKey(sessionService.SessionToken!);
            var secrets = await secretService.GetSecrets(encryptionKey!, await sessionService.GetUserKeyAsync());
            var model = new HomeModel
            {
                SearchString = "",
                Secrets = new SecretListModel
                {
                    Values = secrets
                    .Select(ViewSecret.FromSecret)
                    .ToList()
                }
            };
            return await Create(model, responseOptions);
        }

        public Task<PageEntry> Create(IPageModel model, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {
            return Task.FromResult(new PageEntry
            {
                Page = Page,
                ViewPath = ViewPath,
                ConfigureResponse = PageEntryFactory.CombineResponseOptions(o => o.ConfigureForPage(Page), responseOptions),
                Model = model
            });
        }
    }
}
