using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core
{
    public class PageRegistry(IEnumerable<IPageEntryFactory> pageEntryFactories)
    {
        private readonly IReadOnlyDictionary<string, IPageEntryFactory> _pageFactories = pageEntryFactories
                .GroupBy(f => f.Page, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(grp => grp.Key, grp => grp.Last(), StringComparer.OrdinalIgnoreCase);

        public bool TryGetPageFactory(string page, [NotNullWhen(true)] out WrappedPageEntryFactory? entry)
        {
            if(!_pageFactories.TryGetValue(page, out var pageEntryFactory))
            {
                entry = null;
                return false;
            }
            entry = new WrappedPageEntryFactory(pageEntryFactory, this);
            return true;
        }
        public WrappedPageEntryFactory GetPageFactory(string page)
        {
            var pageEntryFactory = _pageFactories[page];
            return new WrappedPageEntryFactory(pageEntryFactory, this);
        }
    }
}
