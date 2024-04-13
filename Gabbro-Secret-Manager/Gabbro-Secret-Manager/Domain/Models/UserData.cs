using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class UserData
    {
        public static StorageKey<UserData> GetStorageKey(StorageKey<User> userKey) => userKey.Extend<UserData>("");
        public required StorageKey<User> Owner { get; set; }
        public required EncryptionKeySettings EncryptionKeySettings { get; set; }
    }
}
