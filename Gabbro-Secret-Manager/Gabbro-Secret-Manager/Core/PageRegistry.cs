namespace Gabbro_Secret_Manager.Core
{
    public class PageRegistry
    {
        private readonly IReadOnlyDictionary<string, PageRegistryEntry> _pages;
        private readonly IReadOnlyDictionary<string, PageRegistryEntry> _partials;

        public PageRegistry(IEnumerable<PageRegistryEntry> pageEntries)
        {

            _pages = pageEntries
                .Where(p => p.Type == PageEntryType.Page)
                .ToDictionary(p => p.Page, p => p, StringComparer.OrdinalIgnoreCase);
            _partials = pageEntries
                .Where(p => p.Type == PageEntryType.Partial)
                .ToDictionary(p => p.Page, p => p, StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetPage(string page, out PageRegistryEntry? entry) => _pages.TryGetValue(page, out entry);
        public PageRegistryEntry GetPage(string page) => _pages[page];
        public bool TryGetPartialPage(string page, out PageRegistryEntry? entry) => _partials.TryGetValue(page, out entry);
        public PageRegistryEntry GetPartialPage(string page) => _partials[page];
    }
}
