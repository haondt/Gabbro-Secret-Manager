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

        public async Task<PageEntry> Create(IRequestData data)
        {
            var model = new HomeModel
            {
                SearchString = "",
                ShouldReRequestPassword = true
            };

            if (encryptionKeyService.TryGet(sessionService.SessionToken!, out var encryptionKey))
            {
                model.ShouldReRequestPassword = false;
                var secrets = await secretService.GetSecrets(encryptionKey!, await sessionService.GetUserKeyAsync());
                model.Secrets = new SecretListModel
                {
                    Values = secrets
                        .Select(s => new ViewSecret
                        {
                            Name = s.Key,
                            Value = s.Value,
                            Tags = s.Tags
                        })
                        .ToList()
                };
            }

            return await Create(model);
        }

        public Task<PageEntry> Create(object model)
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
