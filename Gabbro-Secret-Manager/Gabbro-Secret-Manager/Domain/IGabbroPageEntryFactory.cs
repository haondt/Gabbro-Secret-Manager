using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Domain
{
    public interface IGabbroPageEntryFactory : IPageEntryFactory
    {
        public bool RequiresEncryptionKey { get; }
    }
}
