using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class UpsertSecretFormFactory(
        ISessionService sessionService,
        EncryptionKeyService encryptionKeyService,
        SecretService secretService) : IPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public string Page => "upsertSecretForm";
        public string ViewPath => "UpsertSecretForm";

        public async Task<PageEntry> Create(PageRegistry pageRegistry, IRequestData data)
        {
            if (encryptionKeyService.TryGet(sessionService.SessionToken!, out var encryptionKey))
            {
                var userKey = await sessionService.GetUserKeyAsync();
                var availableTags = await secretService.GetAvailableTags(encryptionKey, userKey);

                var model = new UpsertSecretFormModel
                {
                    CurrentKey = data.Query.GetValueOrDefault<string?>("currentKey", null),
                    TagSuggestions = availableTags,
                    Key = data.Query.GetValueOrDefault("key", ""),
                    Value = data.Query.GetValueOrDefault("value", ""),
                    Comments = data.Query.GetValueOrDefault("comments", ""),
                    Tags = data.Query.GetValues<string>("tags").Distinct().ToHashSet(),
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
                Model = model,
                SetUrl = ""
            });
        }
    }
}
