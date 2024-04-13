using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class Secret
    {
        public static StorageKey<Secret> GetStorageKey(StorageKey<User> userKey, string name) => userKey.Extend<Secret>(name);

        public required HashSet<string> Tags { get; set; }
        public required string Name { get; set; }
        public required string EncryptedValue { get; set; }
        public required StorageKey<User> Owner { get; set; }
        public required string InitializationVector { get; set; }
        public required string Comments { get; set; }
    }
}
