using Gabbro_Secret_Manager.Core;
using System.Diagnostics.CodeAnalysis;

namespace Gabbro_Secret_Manager.Domain
{
    public interface IGabbroPageRegistry
    {
        public bool TryGetPageFactory(string page, [NotNullWhen(true)] out IGabbroInjectedPageEntryFactory? entry);

        public IGabbroInjectedPageEntryFactory GetPageFactory(string page);
    }
}
