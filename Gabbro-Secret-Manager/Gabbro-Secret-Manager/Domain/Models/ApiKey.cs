using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Domain.Models
{
    public class ApiKey
    {
        public static StorageKey GetStorageKey(Guid id) => id.ToString().GetStorageKey<ApiKey>();
        public required StorageKey Owner { get; set; }
        public required string Name { get; set; }
        public required DateTime Created { get; set; }
        public Guid Id { get; set; }
    }
}
