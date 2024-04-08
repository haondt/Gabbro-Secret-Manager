using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Domain
{
    public class GabbroInjectedPageEntryFactory(IGabbroPageEntryFactory pageEntryFactory, IPageRegistry pageRegistry) : InjectedPageEntryFactory(pageEntryFactory, pageRegistry), IGabbroInjectedPageEntryFactory
    {
        private readonly IGabbroPageEntryFactory _pageEntryFactory = pageEntryFactory;

        public bool RequiresEncryptionKey => _pageEntryFactory.RequiresEncryptionKey;
    }
}
