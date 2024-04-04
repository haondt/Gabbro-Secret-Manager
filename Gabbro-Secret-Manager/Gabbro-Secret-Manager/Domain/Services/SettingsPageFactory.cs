using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SettingsPageFactory(ISessionService sessionService, ApiKeyService apiKeyService) : IPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public string Page => "settings";
        public string ViewPath => "Settings";

        public async Task<PageEntry> Create(PageRegistry pageRegistry, IRequestData data)
        {
            var userKey =  await sessionService.GetUserKeyAsync();
            var apiKeys = await apiKeyService.GetApiKeys(userKey);

            var model = new SettingsModel
            {
                ApiKeys = apiKeys.Select(kvp => new ViewApiKey
                {
                    Id = kvp.Key,
                    Name = kvp.Value.Name,
                }).ToList()
            };
            return await Create(model);
        }

        public Task<PageEntry> Create(IPageModel model)
        {
            return Task.FromResult(new PageEntry
            {
                Page = Page,
                ViewPath = ViewPath,
                SetUrl = "settings",
                Model = model
            });
        }
    }
}
