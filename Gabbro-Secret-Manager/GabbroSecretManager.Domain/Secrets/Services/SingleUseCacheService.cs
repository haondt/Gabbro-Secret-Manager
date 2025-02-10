using Haondt.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace GabbroSecretManager.Domain.Secrets.Services
{
    internal class SingleUseCacheService<T>(IMemoryCache memoryCache) : ISingleUseCacheService<T>
        where T : notnull
    {
        public static TimeSpan Lifetime = TimeSpan.FromMinutes(1);

        public void CacheObject(string key, T value)
        {
            memoryCache.Set(key, value, Lifetime);
        }

        public Optional<T> TryGetObject(string key)
        {
            if (memoryCache.TryGetValue<T>(key, out var value))
            {
                memoryCache.Remove(key);
                return value!;
            }
            return new();
        }
        public Optional<T> TryPeekObject(string key)
        {
            if (memoryCache.TryGetValue<T>(key, out var value))
                return value!;
            return new();
        }

        public void ConsumeObject(string key)
        {
            memoryCache.Remove(key);
        }
    }
}
