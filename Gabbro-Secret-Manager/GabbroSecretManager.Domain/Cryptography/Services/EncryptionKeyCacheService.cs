using GabbroSecretManager.Core.Models;
using GabbroSecretManager.Domain.Cryptography.Models;
using Haondt.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace GabbroSecretManager.Domain.Cryptography.Services
{
    public class EncryptionKeyCacheService(
        IMemoryCache memoryCache,
        IOptions<EncryptionKeyCacheSettings> options) : IEncryptionKeyCacheService
    {
        public byte[] GenerateEncryptionKey(string normalizedUsername, string password, EncryptionKeySettings keySettings)
        {
            var usernameHash = Crypto.GenerateHash(normalizedUsername);
            var passwordHash = Crypto.GenerateHash(password);
            var keyBytes = Crypto.GenerateHash(
                Convert.ToBase64String(usernameHash.Concat(passwordHash).ToArray()),
                Convert.FromBase64String(keySettings.Salt),
                keySettings.Iterations,
                32);
            return keyBytes;
        }

        public Optional<byte[]> TryGetCachedEncryptionKey(string normalizedUsername)
        {
            if (memoryCache.TryGetValue(normalizedUsername, out var key) && key is byte[] casted)
                return casted;
            return new();
        }

        public byte[] GenerateAndCacheEncryptionKey(string noramlizedUsername, string password, EncryptionKeySettings keySettings)
        {
            var encryptionKey = GenerateEncryptionKey(noramlizedUsername, password, keySettings);
            memoryCache.Set(noramlizedUsername, encryptionKey,
                absoluteExpiration: DateTime.UtcNow.AddMinutes(options.Value.LifetimeMinutes));
            return encryptionKey;
        }
        public void ClearCachedEncryptionKey(string noramlizedUsername)
        {
            memoryCache.Remove(noramlizedUsername);
        }
    }
}
