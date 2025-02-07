using Haondt.Core.Models;

namespace GabbroSecretManager.Domain.Secrets.Services
{
    public interface ISingleUseCacheService<T> where T : notnull
    {
        void CacheObject(string key, T value);
        Optional<T> TryGetObject(string key);
        Optional<T> TryPeekObject(string key);
        void ConsumeObject(string key);
    }
}