
namespace Gabbro_Secret_Manager.Core
{
    public class PageEntryFactory : IPageEntryFactory
    {
        public bool RequiresAuthentication { get; set; } = false;
        public required string Page { get; set; }
        public string? SetUrl { get; set; }
        public required string ViewPath { get; set; }
        public required Func<PageRegistry, IRequestData, IPageModel> ModelFactory { get; set; }

        public Task<PageEntry> Create(PageRegistry pageRegistry, IRequestData data) => Create(ModelFactory(pageRegistry, data));

        public Task<PageEntry> Create(IPageModel model)
        {
            return Task.FromResult(new PageEntry
            {
                SetUrl = SetUrl,
                Page = Page,
                ViewPath = ViewPath,
                Model = model
            });
        }
    }
}
