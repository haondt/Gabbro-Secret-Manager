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

        public async Task<DecryptedSecret> GetSecret(byte[] encryptionKey, StorageKey<Secret> secretKey)
        {
            var secret = await storage.Get<Secret>(secretKey);
            var decryptedValue = CryptoService.Decrypt(
                secret.EncryptedValue,
                encryptionKey,
                secret.InitializationVector);
            return secret.AsDecrypted(decryptedValue);
        }

        public async Task<(bool Success, DecryptedSecret?)> TryGetSecret(byte[] encryptionKey, StorageKey<Secret> secretKey)
        {
            var (hasSecret, secret) = await storage.TryGet<Secret>(secretKey);
            if (!hasSecret)
                return (false, null);

            var decryptedValue = CryptoService.Decrypt(
                secret!.EncryptedValue,
                encryptionKey,
                secret.InitializationVector);

            return (true, secret.AsDecrypted(decryptedValue));
        }

        public async Task UpsertSecret(byte[] encryptionKey, StorageKey<User> userKey, string key, string value, string comments, HashSet<string>? tags = null, Guid? id = null)
        {
            id ??= Guid.NewGuid();
            memoryCache.Remove(userKey);

            var secretKey = Secret.GetStorageKey(userKey, id.Value);
            var (encryptedValue, initializationVector) = CryptoService.Encrypt(value, encryptionKey);
            var secret = new Secret
            {
                Id = id.Value,
                Comments = comments,
                Tags = tags ?? [],
                EncryptedValue = encryptedValue,
                InitializationVector = initializationVector,
                Owner = userKey,
                Name = key,
            };
            await storage.Set(secretKey, secret);
        }

        public async Task DeleteSecret(StorageKey<User> userKey, Guid id)
        {
            memoryCache.Remove(userKey);
            var secretKey = Secret.GetStorageKey(userKey, id);
            await storage.Delete(secretKey);
        }

        public async Task<List<DecryptedSecret>> GetSecrets(byte[] encryptionKey, StorageKey<User> userKey, string? secretName = null, IReadOnlyCollection<string>? tags = null)
        {
            var secrets = await storage.GetSecrets(userKey, secretName, tags);

            return secrets.Select(s =>
            {
                var decryptedValue = CryptoService.Decrypt(
                    s.EncryptedValue,
                    encryptionKey,
                    s.InitializationVector);
                return s.AsDecrypted(decryptedValue);
            }).ToList();
        }

    }
}
