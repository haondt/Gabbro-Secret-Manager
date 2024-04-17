using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class Secret
    {
        public static StorageKey<Secret> GetStorageKey(StorageKey<User> userKey, Guid id) => userKey.Extend<Secret>(id.ToString());

        public required Guid Id { get; set; }
        public required HashSet<string> Tags { get; set; }
        public required string Name { get; set; }
        public required string EncryptedValue { get; set; }
        public required StorageKey<User> Owner { get; set; }
        public required string InitializationVector { get; set; }
        public required string Comments { get; set; }
    }
}
