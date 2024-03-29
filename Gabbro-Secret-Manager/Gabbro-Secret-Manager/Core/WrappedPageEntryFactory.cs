
namespace Gabbro_Secret_Manager.Core
{
    public class WrappedPageEntryFactory(IPageEntryFactory pageEntryFactory, PageRegistry pageRegistry)
    {
        public bool RequiresAuthentication => pageEntryFactory.RequiresAuthentication;

        public string Page => pageEntryFactory.Page;

        public string ViewPath => pageEntryFactory.ViewPath;

        public Task<PageEntry> Create(IRequestData data)
            => pageEntryFactory.Create(pageRegistry, data);

        public Task<PageEntry> Create(IPageModel model) => pageEntryFactory.Create(model);
    }
}
