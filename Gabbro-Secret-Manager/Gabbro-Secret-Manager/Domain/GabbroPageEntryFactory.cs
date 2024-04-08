using Gabbro_Secret_Manager.Core;

namespace Gabbro_Secret_Manager.Domain
{
    public class GabbroPageEntryFactory : PageEntryFactory, IGabbroPageEntryFactory
    {
        public bool RequiresEncryptionKey { get; init; } = false;
    }
}
