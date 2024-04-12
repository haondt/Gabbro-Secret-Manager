using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class ApiKeyService(IGabbroStorageService storage, JweService jweService)
    {
        public async Task<(string Token, ApiKey ApiKey)> CreateApiTokenAsync(StorageKey userKey, string name, byte[] encryptionKey)
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

        public Task<Dictionary<StorageKey, ApiKey>> GetApiKeys(StorageKey userKey) => storage.GetApiKeys(userKey);

        public Task DeleteApiKey(Guid apiKeyId)
        {
            var apiKeyKey = ApiKey.GetStorageKey(apiKeyId);
            return storage.Delete(apiKeyKey);
        }

        public async Task<bool> VerifyOwner(StorageKey userKey, Guid apiKeyId)
        {
            var apiKeyKey = ApiKey.GetStorageKey(apiKeyId);
            if (await storage.TryGet<ApiKey>(apiKeyKey) is not (true, var apiKey))
                return false;

            return apiKey!.Owner.Equals(userKey);
        }

        public async Task<(bool IsValid, StorageKey userKey, string encryptionKey)> ValidateApiTokenAsync(string token)
        {
            if (!await jweService.IsValid(token)) 
                return (false, StorageKey.Empty, "");
            var claims = await jweService.GetClaims(token);
            var id = Guid.Parse(claims["id"]);
            var apiKeyKey = ApiKey.GetStorageKey(id);
            var encryptionKey = claims["encryptionKey"];
            var type = claims["type"];

            if (type != "apiToken")
                return (false, StorageKey.Empty, "");

            if (await storage.TryGet<ApiKey>(apiKeyKey) is not (true, var apiKey))
                return (false, StorageKey.Empty, "");

            return (true, apiKey!.Owner, encryptionKey);
        }

    }
}
