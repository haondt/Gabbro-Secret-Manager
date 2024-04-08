using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core
{
    public class PageRegistry(IEnumerable<IPageEntryFactory> pageEntryFactories) : IPageRegistry
    {
        private readonly IReadOnlyDictionary<string, IPageEntryFactory> _pageFactories = pageEntryFactories
                .GroupBy(f => f.Page, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(grp => grp.Key, grp => grp.Last(), StringComparer.OrdinalIgnoreCase);

        public bool TryGetPageFactory(string page, [NotNullWhen(true)] out IInjectedPageEntryFactory? entry)
        {
            if(!_pageFactories.TryGetValue(page, out var pageEntryFactory))
            {
                entry = null;
                return false;
            }
            entry = new InjectedPageEntryFactory(pageEntryFactory, this);
            return true;
        }
        public IInjectedPageEntryFactory GetPageFactory(string page)
        {
            var pageEntryFactory = _pageFactories[page];
            return new InjectedPageEntryFactory(pageEntryFactory, this);
        }
    }
}
