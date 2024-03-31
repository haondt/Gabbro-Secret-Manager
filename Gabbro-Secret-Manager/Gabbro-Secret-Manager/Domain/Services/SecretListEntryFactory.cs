using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SecretListEntryFactory(
        ISessionService sessionService,
        EncryptionKeyService encryptionKeyService,
        SecretService secretService) : IPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public string Page => "secretListEntry";
        public string ViewPath => "SecretListEntry";

        public async Task<PageEntry> Create(PageRegistry pageRegistry, IRequestData data)
        {
            var secretName = data.Query.GetValue<string>(SecretListEntryModel.SecretNameKey); 

            
            if (encryptionKeyService.TryGet(sessionService.SessionToken!, out var encryptionKey))
            {
                var (secret, tags) = await secretService.GetSecret(encryptionKey!, await sessionService.GetUserKeyAsync(), secretName);
                var model = new SecretListEntryModel
                {
                    Secret = new ViewSecret
                    {
                        Name = secretName,
                        Value = secret,
                        Tags = tags,
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
                Model = model
            });
        }
    }
}
