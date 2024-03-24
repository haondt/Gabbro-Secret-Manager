using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class SecretService(IGabbroStorageService storage, UserService userService, EncryptionKeyService encryptionKeyService)
    {
        public async Task<string> GetSecret(string sessionToken, string key)
        {
            var session = await userService.GetSession(sessionToken);
            var encryptionKey = encryptionKeyService.Get(sessionToken);
            return await GetSecret(encryptionKey, session.UserKey, key);
        }

        public Task<string> GetSecret(string encryptionKey, string userKey, string key) => GetSecret(
            Convert.FromBase64String(encryptionKey), userKey, key);
        public async Task<string> GetSecret(byte[] encryptionKey, string userKey, string key)
        {
            var secretKey = userKey + "___" + key.GetStorageKey<string>();
            var secret = await storage.Get<Secret>(secretKey);
            var decryptedValue = CryptoService.Decrypt(
                secret.EncryptedValue,
                encryptionKey,
                secret.InitializationVector);
            return decryptedValue;
        }

        public async Task UpsertSecret(string sessionToken, string key, string value)
        {
            var session = await userService.GetSession(sessionToken);
            var secretKey = session.UserKey + "___" + key.GetStorageKey<string>();
            var encryptionKey = encryptionKeyService.Get(sessionToken);
            var (encryptedValue, initializationVector) = CryptoService.Encrypt(value, encryptionKey);
            var secret = new Secret
            {
                EncryptedValue = encryptedValue,
                InitializationVector = initializationVector,
                Owner = session.UserKey,
                Name = key,
            };
            await storage.Set(secretKey, secret);
        }

        public async Task DeleteSecret(string sessionToken, string key, string value)
        {
            var session = await userService.GetSession(sessionToken);
            var secretKey = session.UserKey + "___" + key.GetStorageKey<string>();
            await storage.Delete(secretKey);
        }

        public async Task<List<(string Key, string Value)>> GetSecrets(string sessionToken)
        {
            var session = await userService.GetSession(sessionToken);
            var encryptionKey = encryptionKeyService.Get(sessionToken);
            var secrets = await storage.GetSecrets(session.UserKey);

            return secrets.Select(s =>
            {
                var decryptedValue = CryptoService.Decrypt(
                    s.EncryptedValue,
                    encryptionKey,
                    s.InitializationVector);
                return (s.Name, decryptedValue);
            }).ToList();
        }
    }
}
