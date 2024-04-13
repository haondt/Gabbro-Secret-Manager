using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class ApiKey
    {
        public static StorageKey<ApiKey> GetStorageKey(Guid id) => new(id.ToString());
        public required StorageKey<User> Owner { get; set; }
        public required string Name { get; set; }
        public required DateTime Created { get; set; }
        public Guid Id { get; set; }
    }
}
