using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;

namespace Gabbro_Secret_Manager.Domain.Services
{
    public class ApiKeyService(IGabbroStorageService storage, JweService jweService)
    {
        public async Task<(string Token, string ApiKeyId, ApiKey ApiKey)> CreateApiTokenAsync(string userKey, string name, byte[] encryptionKey)
        {
            var id = Guid.NewGuid().ToString();
            var apiKeyKey = id.GetStorageKey<ApiKey>();
            var apiKey = new ApiKey { Owner = userKey, Name = name };
            await storage.Set(apiKeyKey, apiKey);
            var claims = new Dictionary<string, string>
            {
                { "id", id },
                { "encryptionKey", Convert.ToBase64String(encryptionKey) },
                { "type", "apiToken" }
            };

            var token = jweService.CreateToken(claims);
            return (token, id, apiKey);
        }

        public Task<Dictionary<string, ApiKey>> GetApiKeys(string userKey) => storage.GetApiKeys(userKey);

        public Task DeleteApiKey(string apiKeyId)
        {
            var apiKeyKey = apiKeyId.GetStorageKey<ApiKey>();
            return storage.Delete(apiKeyKey);
        }

        public async Task<(bool IsValid, string userKey, string encryptionKey)> ValidateApiTokenAsync(string token)
        {
            if (!await jweService.IsValid(token)) 
                return (false, "", "");
            var claims = await jweService.GetClaims(token);
            var id = claims["id"];
            var apiKeyKey = id.GetStorageKey<ApiKey>();
            var encryptionKey = claims["encryptionKey"];
            var type = claims["type"];

            if (type != "apiToken")
                return (false, "", "");

            if (await storage.TryGet<ApiKey>(apiKeyKey) is not (true, var apiKey))
                return (false, "", "");

            return (true, apiKey!.Owner, encryptionKey);
        }

    }
}
