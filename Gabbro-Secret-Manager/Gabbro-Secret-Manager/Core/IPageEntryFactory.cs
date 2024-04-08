namespace Gabbro_Secret_Manager.Core
{
    public interface IPageEntryFactory
    {
        public bool RequiresAuthentication { get; }
        public string Page { get; }
        public string ViewPath { get; }
        public Task<PageEntry> Create(IPageRegistry pageRegistry, IRequestData data, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null);

        public Task<PageEntry> Create(IPageModel model, Func<HxHeaderBuilder, HxHeaderBuilder>? responseOptions = null);
    }
}
