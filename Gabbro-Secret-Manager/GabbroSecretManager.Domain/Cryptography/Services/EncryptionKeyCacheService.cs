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
        private byte[] GenerateEncryptionKey(string normalizedUsername, string password, EncryptionKeySettings keySettings)
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

        public Optional<byte[]> TryGetEncryptionKey(string normalizedUsername)
        {
            if (memoryCache.TryGetValue(normalizedUsername, out var key) && key is byte[] casted)
                return casted;
            return new();
        }

        public byte[] CreateEncryptionKey(string noramlizedUsername, string password, EncryptionKeySettings keySettings)
        {
            var encryptionKey = GenerateEncryptionKey(noramlizedUsername, password, keySettings);
            memoryCache.Set(noramlizedUsername, encryptionKey,
                absoluteExpiration: DateTime.UtcNow.AddMinutes(options.Value.LifetimeMinutes));
            return encryptionKey;
        }
        public void ClearEncryptionKey(string noramlizedUsername)
        {
            memoryCache.Remove(noramlizedUsername);
        }
    }
}
