using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;
using Microsoft.Extensions.Caching.Memory;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SecretService(IGabbroStorageService storage, IMemoryCache memoryCache)
    {
        public Task<List<string>> GetAvailableTags(byte[] encryptionKey, StorageKey<User> userKey)
        {
            return memoryCache.GetOrCreateAsync(userKey, async e =>
            {
                e.SlidingExpiration = TimeSpan.FromHours(1);
                var secrets = await GetSecrets(encryptionKey, userKey);
                return secrets.SelectMany(s => s.Tags).Distinct().ToList();
            })!;
        }

        public async Task<(string Value, HashSet<string> Tags)> GetSecret(byte[] encryptionKey, StorageKey<Secret> secretKey)
        {
            var secret = await storage.Get<Secret>(secretKey);
            var decryptedValue = CryptoService.Decrypt(
                secret.EncryptedValue,
                encryptionKey,
                secret.InitializationVector);
            return (decryptedValue, secret.Tags);
        }

        public async Task<(bool Success, string Value, string comments, HashSet<string> Tags)> TryGetSecret(byte[] encryptionKey, StorageKey<Secret> secretKey)
        {
            var (hasSecret, secret) = await storage.TryGet<Secret>(secretKey);
            if (!hasSecret)
                return (false, "", "", []);

            var decryptedValue = CryptoService.Decrypt(
                secret!.EncryptedValue,
                encryptionKey,
                secret.InitializationVector);

            return (true, decryptedValue, secret.Comments, secret.Tags);
        }

        public async Task UpsertSecret(byte[] encryptionKey, StorageKey<User> userKey, string key, string value, string comments, HashSet<string>? tags = null)
        {
            memoryCache.Remove(userKey);
            var secretKey = Secret.GetStorageKey(userKey, key);
            var (encryptedValue, initializationVector) = CryptoService.Encrypt(value, encryptionKey);
            var secret = new Secret
            {
                Comments = comments,
                Tags = tags ?? [],
                EncryptedValue = encryptedValue,
                InitializationVector = initializationVector,
                Owner = userKey,
                Name = key,
            };
            await storage.Set(secretKey, secret);
        }

        public Task<bool> ContainsSecret(StorageKey<User> userKey, string key)
        {
            var secretKey = Secret.GetStorageKey(userKey, key);
            return storage.ContainsKey(secretKey);
        }

        public async Task DeleteSecret(StorageKey<User> userKey, string key)
        {
            memoryCache.Remove(userKey);
            var secretKey = Secret.GetStorageKey(userKey, key);
            await storage.Delete(secretKey);
        }

        public async Task<List<(string Key, string Value, HashSet<string> Tags)>> GetSecrets(byte[] encryptionKey, StorageKey<User> userKey)
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
