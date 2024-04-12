using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class UserData
    {
        public required StorageKey Owner { get; set; }
        public required EncryptionKeySettings EncryptionKeySettings { get; set; }
    }
}
