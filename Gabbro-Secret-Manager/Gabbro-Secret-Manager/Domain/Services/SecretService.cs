using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;
using Microsoft.Extensions.Caching.Memory;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SecretService(IGabbroStorageService storage, IMemoryCache memoryCache)
    {
        public Task<List<string>> GetAvailableTags(byte[] encryptionKey, string userKey)
        {
            return memoryCache.GetOrCreateAsync(userKey, async e =>
            {
                e.SlidingExpiration = TimeSpan.FromHours(1);
                var secrets = await GetSecrets(encryptionKey, userKey);
                return secrets.SelectMany(s => s.Tags).Distinct().ToList();
            })!;
        }

        public async Task<(string Value, HashSet<string> Tags)> GetSecret(byte[] encryptionKey, string userKey, string key)
        {
            var secretKey = userKey + "___" + key.GetStorageKey<string>();
            var secret = await storage.Get<Secret>(secretKey);
            var decryptedValue = CryptoService.Decrypt(
                secret.EncryptedValue,
                encryptionKey,
                secret.InitializationVector);
            return (decryptedValue, secret.Tags);
        }

        public async Task UpsertSecret(byte[] encryptionKey, string userKey, string key, string value, HashSet<string>? tags = null)
        {
            memoryCache.Remove(userKey);
            var secretKey = userKey + "___" + key.GetStorageKey<string>();
            var (encryptedValue, initializationVector) = CryptoService.Encrypt(value, encryptionKey);
            var secret = new Secret
            {
                Tags = tags ?? [],
                EncryptedValue = encryptedValue,
                InitializationVector = initializationVector,
                Owner = userKey,
                Name = key,
            };
            await storage.Set(secretKey, secret);
        }

        public Task<bool> ContainsSecret(string userKey, string key)
        {
            var secretKey = userKey + "___" + key.GetStorageKey<string>();
            return storage.ContainsKey(secretKey);
        }

        public async Task DeleteSecret(string userKey, string key)
        {
            memoryCache.Remove(userKey);
            var secretKey = userKey + "___" + key.GetStorageKey<string>();
            await storage.Delete(secretKey);
        }

        public async Task<List<(string Key, string Value, HashSet<string> Tags)>> GetSecrets(byte[] encryptionKey, string userKey)
        {
            var secrets = await storage.GetSecrets(userKey);

            return secrets.Select(s =>
            {
                var decryptedValue = CryptoService.Decrypt(
                    s.EncryptedValue,
                    encryptionKey,
                    s.InitializationVector);
                return (s.Name, decryptedValue, s.Tags);
            }).ToList();
        }

    }
}
