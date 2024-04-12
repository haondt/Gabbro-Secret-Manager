using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SettingsPageFactory(ISessionService sessionService, ApiKeyService apiKeyService) : IPageEntryFactory
    {
        public bool RequiresAuthentication => true;
        public string Page => "settings";
        public string ViewPath => "Settings";

        public async Task<PageEntry> Create(IPageRegistry pageRegistry, IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
        {
            var userKey =  await sessionService.GetUserKeyAsync();
            var apiKeys = await apiKeyService.GetApiKeys(userKey);

            var model = new SettingsModel
            {
                ApiKeys = apiKeys
                .OrderByDescending(kvp => kvp.Value.Created)
                .Select(kvp => new ViewApiKey
                {
                    Id = kvp.Value.Id.ToString(),
                    Name = kvp.Value.Name,
                }).ToList()
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
