using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Domain
{
    public interface IGabbroInjectedPageEntryFactory : IInjectedPageEntryFactory
    {
        public bool RequiresEncryptionKey { get; }
    }
}
