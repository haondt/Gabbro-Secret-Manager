using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class HomePageEntryFactory(ISessionService sessionService, EncryptionKeyService encryptionKeyService, SecretService secretService) : IPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public bool IsPage => true;
        public string Page => "home";
        public string ViewPath => "Home";

        public async Task<PageEntry> Create(PageRegistry pageRegistry, IRequestData data)
        {

            if (encryptionKeyService.TryGet(sessionService.SessionToken!, out var encryptionKey))
            {
                var secrets = await secretService.GetSecrets(encryptionKey!, await sessionService.GetUserKeyAsync());
                var model = new HomeModel
                {
                    SearchString = "",
                    Secrets = new SecretListModel
                    {
                        Values = secrets
                        .Select(s => new ViewSecret
                        {
                            Name = s.Key,
                            Value = s.Value,
                            Tags = s.Tags
                        })
                        .ToList()
                    }
                };
                return await Create(model);
            }

            return await pageRegistry.GetPageFactory("passwordReentryForm").Create(data);
        }

        public Task<PageEntry> Create(IPageModel model)
        {
            return Task.FromResult(new PageEntry
            {
                Page = Page,
                ViewPath = ViewPath,
                SetUrl = "home",
                Model = model
            });
        }
    }
}
