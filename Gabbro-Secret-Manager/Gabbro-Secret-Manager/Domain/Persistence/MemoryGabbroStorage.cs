using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;

namespace Gabbro_Secret_Manager.Domain.Persistence
{
    public class MemoryGabbroStorage : MemoryStorage, IGabbroStorage
    {
        public Task<List<Secret>> GetSecrets(StorageKey<User> userKey)
        {
            var secrets = _storage
                .Select(kvp => kvp.Value as Secret)
                .Where(s => s != null && s.Owner.Equals(userKey))
                .Cast<Secret>()
                .ToList();
            return Task.FromResult(secrets);
        }

        public Task<Dictionary<StorageKey<ApiKey>, ApiKey>> GetApiKeys(StorageKey<User> userKey)
        {
            var apiKeys = _storage
                    .Where(kvp => kvp.Key.Type == typeof(ApiKey))
                    .Where(kvp => kvp.Value != null && kvp.Value is ApiKey)
                    .Select(kvp => (kvp.Key.As<ApiKey>(), (kvp.Value as ApiKey)!))
                    .Where(t => t.Item2.Owner.Equals(userKey))
                .ToDictionary(t => t.Item1, t => t.Item2);
            return Task.FromResult(apiKeys);
        }
    }
}
