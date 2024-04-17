using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
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
            var encryptionKey = encryptionKeyService.GetEncryptionKey(sessionService.SessionToken!);
            var userKey = await sessionService.GetUserKeyAsync();
            var availableTags = await secretService.GetAvailableTags(encryptionKey, userKey);

            var secretId = data.Query.GetValueOrDefault<Guid?>("id", null);

            var model = await CreateModel(availableTags, secretId, encryptionKey, userKey);
            return await Create(model, responseOptions);
        }

        private async Task<UpsertSecretFormModel> CreateModel(List<string> availableTags, Guid? secretId, byte[] encryptionKey, StorageKey<User> userKey)
        {
            var model = new UpsertSecretFormModel { TagSuggestions = availableTags };
            if (!secretId.HasValue)
                return model;

            var (existsSecret, secret) = await secretService.TryGetSecret(encryptionKey, Secret.GetStorageKey(userKey, secretId.Value));
            if (!existsSecret || secret == null) 
                return model;

            model.Name = secret.Name;
            model.Id = secret.Id;
            model.Value = secret.Value;
            model.Comments = secret.Comments;
            model.Tags = secret.Tags;
            return model;
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
