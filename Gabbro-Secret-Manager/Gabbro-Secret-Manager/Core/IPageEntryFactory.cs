namespace Gabbro_Secret_Manager.Core
{
    public interface IPageEntryFactory
    {
        public bool RequiresAuthentication { get; }
        public string Page { get; }
        public string ViewPath { get; }
        public Task<PageEntry> Create(IRequestData data);
        public Task<PageEntry> Create(object model);
    }
}
