using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public interface IGabbroStorage : IStorage
    {
        public Task<List<Secret>> GetSecrets(StorageKey<User> userKey, string? secretName, IReadOnlyCollection<string>? tags = null);
        public Task<Dictionary<StorageKey<ApiKey>, ApiKey>> GetApiKeys(StorageKey<User> userKey);
    }
}
