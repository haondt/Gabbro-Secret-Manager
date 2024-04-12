using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public class UserSession
    {
        public static StorageKey GetStorageKey(string sessionToken) => sessionToken.GetStorageKey<UserSession>();
        public required DateTime Expiry { get; set; }
        public required StorageKey Owner { get; set; }
    }
}
