namespace Gabbro_Secret_Manager.Core
{
    public class InjectedPageEntryFactory(IPageEntryFactory pageEntryFactory, IPageRegistry pageRegistry) : IInjectedPageEntryFactory
    {
        public bool RequiresAuthentication => pageEntryFactory.RequiresAuthentication;

        public string Page => pageEntryFactory.Page;

        public string ViewPath => pageEntryFactory.ViewPath;

        public Task<PageEntry> Create(IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null)
            => pageEntryFactory.Create(pageRegistry, data, responseOptions);

        public Task<PageEntry> Create(IPageModel model, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null) => pageEntryFactory.Create(model, responseOptions);

    }
}
