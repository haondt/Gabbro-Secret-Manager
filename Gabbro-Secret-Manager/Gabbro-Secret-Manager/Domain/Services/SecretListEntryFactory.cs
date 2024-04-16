using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SecretListEntryFactory(
        ISessionService sessionService,
        EncryptionKeyService encryptionKeyService,
        SecretService secretService) : IGabbroPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public bool RequiresEncryptionKey => true;
        public string Page => "secretListEntry";
        public string ViewPath => "SecretListEntry";

        public async Task<PageEntry> Create(IPageRegistry pageRegistry, IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {
            var secretName = data.Query.GetValue<string>(SecretListEntryModel.SecretNameKey);

            var encryptionKey = encryptionKeyService.GetEncryptionKey(sessionService.SessionToken!);

            var userKey = await sessionService.GetUserKeyAsync();
            var secretKey = Secret.GetStorageKey(userKey, secretName);
            var (secret, tags) = await secretService.GetSecret(encryptionKey!, secretKey);
            var model = new SecretListEntryModel
            {
                Secret = new ViewSecret
                {
                    Name = secretName,
                    Value = secret,
                    Tags = tags,
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
                Model = model,
                ConfigureResponse = PageEntryFactory.CombineResponseOptions(responseOptions)
            });
        }
    }
}
