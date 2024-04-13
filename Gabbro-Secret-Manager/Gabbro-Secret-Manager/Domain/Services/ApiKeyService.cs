using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class ApiKeyService(IGabbroStorageService storage, JweService jweService)
    {
        public async Task<(string Token, ApiKey ApiKey)> CreateApiTokenAsync(StorageKey<User> userKey, string name, byte[] encryptionKey)
        {
            var id = Guid.NewGuid();
            var apiKeyKey = ApiKey.GetStorageKey(id);
            var apiKey = new ApiKey { Owner = userKey, Name = name, Created = DateTime.UtcNow, Id = id };
            await storage.Set(apiKeyKey, apiKey);
            var claims = new Dictionary<string, string>
            {
                { "id", id.ToString() },
                { "encryptionKey", Convert.ToBase64String(encryptionKey) },
                { "type", "apiToken" }
            };

            var token = jweService.CreateToken(claims);
            return (token, apiKey);
        }

        public Task<Dictionary<StorageKey<ApiKey>, ApiKey>> GetApiKeys(StorageKey<User> userKey) => storage.GetApiKeys(userKey);

        public Task DeleteApiKey(Guid apiKeyId)
        {
            var apiKeyKey = ApiKey.GetStorageKey(apiKeyId);
            return storage.Delete(apiKeyKey);
        }

        public async Task<bool> VerifyOwner(StorageKey<User> userKey, Guid apiKeyId)
        {
            var apiKeyKey = ApiKey.GetStorageKey(apiKeyId);
            if (await storage.TryGet<ApiKey>(apiKeyKey) is not (true, var apiKey))
                return false;

            return apiKey!.Owner.Equals(userKey);
        }

        public async Task<(bool IsValid, StorageKey<User> userKey, byte[] encryptionKey)> ValidateApiTokenAsync(string token)
        {
            var defaultValue = (false, StorageKey<User>.Empty, Array.Empty<byte>());
            if (!await jweService.IsValid(token)) 
                return defaultValue;
            var claims = await jweService.GetClaims(token);
            var id = Guid.Parse(claims["id"]);
            var apiKeyKey = ApiKey.GetStorageKey(id);
            var encryptionKey = Convert.FromBase64String(claims["encryptionKey"]);
            var type = claims["type"];

            if (type != "apiToken")
                return defaultValue;

            if (await storage.TryGet<ApiKey>(apiKeyKey) is not (true, var apiKey))
                return defaultValue;

            return (true, apiKey!.Owner, encryptionKey);
        }

    }
}
