using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public class UserSession
    {
        public static StorageKey<UserSession> GetStorageKey(string sessionToken) => new(sessionToken);
        public required DateTime Expiry { get; set; }
        public required StorageKey<User> Owner { get; set; }
    }
}
