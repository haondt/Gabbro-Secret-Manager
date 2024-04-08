using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class UpsertSecretFormFactory(
        ISessionService sessionService,
        EncryptionKeyService encryptionKeyService,
        SecretService secretService) : IGabbroPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public string Page => "upsertSecretForm";
        public string ViewPath => "UpsertSecretForm";

        public bool RequiresEncryptionKey => true;

        public async Task<PageEntry> Create(IPageRegistry pageRegistry, IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {
            var encryptionKey = encryptionKeyService.Get(sessionService.SessionToken!);
            var userKey = await sessionService.GetUserKeyAsync();
            var availableTags = await secretService.GetAvailableTags(encryptionKey, userKey);
            var secretKey = data.Query.GetValueOrDefault("key", "");
            var currentKey = data.Query.GetValueOrDefault<string?>("currentKey", null);
            var (existsSecret, secret, comments, secretTags) = await secretService.TryGetSecret(encryptionKey, userKey, secretKey);

            var model = new UpsertSecretFormModel
            {
                CurrentKey = currentKey,
                TagSuggestions = availableTags,
                Key = secretKey,
                Value = existsSecret ? secret : "",
                Comments = existsSecret ? comments : "",
                Tags = existsSecret ? secretTags : []
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
                ConfigureResponse = PageEntryFactory.CombineResponseOptions(o => o.ConfigureForPage(""), responseOptions)
            });
        }
    }
}
