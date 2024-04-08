using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SecretListFactory(ISessionService sessionService, EncryptionKeyService encryptionKeyService, SecretService secretService) : IGabbroPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public bool RequiresEncryptionKey => true;
        public string Page => "secretList";
        public string ViewPath => "SecretList";

        public async Task<PageEntry> Create(IPageRegistry pageRegistry, IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {
            var searchTags = data.Form
                .Where(kvp => kvp.Key != "search")
                .Where(kvp => kvp.Value.Any(v => "on".Equals(v)))
                .Select(kvp => kvp.Key);

            var encryptionKey = encryptionKeyService.Get(sessionService.SessionToken!);
            var secrets = await secretService.GetSecrets(encryptionKey!, await sessionService.GetUserKeyAsync());
            var filteredSecrets = secrets
                    .Where(s => searchTags.All(t => s.Tags.Any(t2 => t.Equals(t2, StringComparison.OrdinalIgnoreCase))));
            if (data.Form.TryGetValue<string>("search", out var searchString))
                filteredSecrets = filteredSecrets.Where(s => s.Key.Contains(searchString, StringComparison.OrdinalIgnoreCase));

            var model = new SecretListModel
            {
                Values = filteredSecrets
                    .Select(s => new ViewSecret
                    {
                        Name = s.Key,
                        Value = s.Value,
                        Tags = s.Tags
                    })
                    .ToList()
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
