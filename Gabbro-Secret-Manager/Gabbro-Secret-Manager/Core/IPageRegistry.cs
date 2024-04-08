using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Core
{
    public interface IPageRegistry
    {

        public bool TryGetPageFactory(string page, [NotNullWhen(true)] out IInjectedPageEntryFactory? entry);

        public IInjectedPageEntryFactory GetPageFactory(string page);
    }
}
