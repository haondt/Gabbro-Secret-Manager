namespace Gabbro_Secret_Manager.Core
{
    public class PageRegistry
    {
        private readonly IReadOnlyDictionary<string, PageRegistryEntry> _pages;

        public PageRegistry(IEnumerable<PageRegistryEntry> pageEntries)
        {

            _pages = pageEntries.ToDictionary(p => p.Page, p => p, StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetPage(string page, out PageRegistryEntry? entry) => _pages.TryGetValue(page, out entry);
        public PageRegistryEntry GetPage(string page) => _pages[page];
    }
}
