using Gabbro_Secret_Manager.Core;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Domain
{
    public class GabbroPageRegistry(
        IEnumerable<IPageEntryFactory> pageEntryFactories,
        IEnumerable<IGabbroPageEntryFactory> gabbroPageEntryFactories) : IPageRegistry, IGabbroPageRegistry
    {
        private readonly ReadOnlyDictionary<string, IGabbroPageEntryFactory> _pageFactories = pageEntryFactories
                .Select(pf => new GabbroPageEntryFactoryWrapper(pf))
                .Concat(gabbroPageEntryFactories)
                .GroupBy(f => f.Page, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(grp => grp.Key, grp => grp.Last(), StringComparer.OrdinalIgnoreCase)
                .AsReadOnly();

        public bool TryGetPageFactory(string page, [NotNullWhen(true)] out IInjectedPageEntryFactory? entry)
        {
            if(!_pageFactories.TryGetValue(page, out var pageEntryFactory))
            {
                entry = null;
                return false;
            }
            entry = new GabbroInjectedPageEntryFactory(pageEntryFactory, this);
            return true;
        }

        public IInjectedPageEntryFactory GetPageFactory(string page)
        {
            var pageEntryFactory = _pageFactories[page];
            return new GabbroInjectedPageEntryFactory(pageEntryFactory, this);
        }

        public bool TryGetPageFactory(string page, [NotNullWhen(true)] out IGabbroInjectedPageEntryFactory? entry)
        {
            if(!_pageFactories.TryGetValue(page, out var pageEntryFactory))
            {
                entry = null;
                return false;
            }
            entry = new GabbroInjectedPageEntryFactory(pageEntryFactory, this);
            return true;
        }

        IGabbroInjectedPageEntryFactory IGabbroPageRegistry.GetPageFactory(string page)
        {
            var pageEntryFactory = _pageFactories[page];
            return new GabbroInjectedPageEntryFactory(pageEntryFactory, this);
        }
    }
}
