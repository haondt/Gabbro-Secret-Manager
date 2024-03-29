using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SecretService(IGabbroStorageService storage)
    {
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

        public async Task UpsertSecret(byte[] encryptionKey, string userKey, string key, string value)
        {
            var secretKey = userKey + "___" + key.GetStorageKey<string>();
            var (encryptedValue, initializationVector) = CryptoService.Encrypt(value, encryptionKey);
            var secret = new Secret
            {
                EncryptedValue = encryptedValue,
                InitializationVector = initializationVector,
                Owner = userKey,
                Name = key,
            };
            await storage.Set(secretKey, secret);
        }

        public async Task DeleteSecret(string userKey, string key)
        {
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
