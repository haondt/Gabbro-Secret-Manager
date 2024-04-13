using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Gabbro_Secret_Manager.Core.Persistence
{
    public class StorageService(IStorage inner, IMemoryCache cache, IOptions<PersistenceSettings> persistenceOptions) : IStorageService
    {
        PersistenceSettings _persistenceSettings = persistenceOptions.Value;

        public Task<bool> ContainsKey(StorageKey key) => inner.ContainsKey(key);
        public Task Delete(StorageKey key)
        {
            if (_persistenceSettings.UseReadCaching)
                cache.Remove(key);
            return inner.Delete(key);
        }

        public async Task<T> Get<T>(StorageKey<T> key)
        {
            if (_persistenceSettings.UseReadCaching)
            {
                if (cache.TryGetValue(key, out var value))
                    return (T)value!;
                var innerValue = await inner.Get<T>(key);
                cache.Set(key, innerValue, DateTime.UtcNow + _persistenceSettings.CacheLifetime);
                return innerValue;
            }
            return await inner.Get<T>(key);
        }

        public Task Set<T>(StorageKey<T> key, T value)
        {
            if (_persistenceSettings.UseReadCaching)
                cache.Remove(key);
            return inner.Set(key, value);
        }

        public async Task<(bool, T?)> TryGet<T>(StorageKey<T> key)
        {
            if (_persistenceSettings.UseReadCaching)
                if (cache.TryGetValue(key, out var value))
                    return (true, (T?)value);
            if (await inner.TryGet<T>(key) is (true, var innerValue))
            {
                if (_persistenceSettings.UseReadCaching)
                    cache.Set(key, innerValue, DateTime.UtcNow + _persistenceSettings.CacheLifetime);
                return (true, innerValue);
            }
            return (false, default);
        }
    }
}
