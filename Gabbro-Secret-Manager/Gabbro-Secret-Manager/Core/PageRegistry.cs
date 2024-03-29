namespace Gabbro_Secret_Manager.Core
{
    public class PageRegistry(IEnumerable<IPageEntryFactory> pageEntryFactories)
    {
        private readonly IReadOnlyDictionary<string, IPageEntryFactory> _pageFactories = pageEntryFactories
                .GroupBy(f => f.Page, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(grp => grp.Key, grp => grp.Last(), StringComparer.OrdinalIgnoreCase);

        public bool TryGetPageFactory(string page, out IPageEntryFactory? entry) => _pageFactories.TryGetValue(page, out entry);
        public IPageEntryFactory GetPageFactory(string page) => _pageFactories[page];
    }
}
