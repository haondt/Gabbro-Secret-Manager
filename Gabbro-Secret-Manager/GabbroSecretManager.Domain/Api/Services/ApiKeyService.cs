using GabbroSecretManager.Core.Models;
using GabbroSecretManager.Persistence.Data;
using Haondt.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GabbroSecretManager.Domain.Api.Services
{
    internal class ApiKeyService(
        IJweService jweService,
        ApiKeyDbContext apiKeyDb) : IApiKeyService
    {

        public async Task<(string Token, ApiKey ApiKey)> CreateApiKey(string name, string normalizedUsername, string password, byte[] encryptionKey)
        {
            var apiKey = new ApiKey()
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Owner = normalizedUsername
            };

            apiKeyDb.ApiKeys.Add(Persistence.Models.ApiKeySurrogate.FromApiKey(apiKey));
            await apiKeyDb.SaveChangesAsync();

            var jwe = jweService.CreateToken(new()
            {
                { "id" , apiKey.Id },
                { "encryptionKey" , Convert.ToBase64String(encryptionKey) },
                { "type" , "apiToken" }
            });

            return (jwe, apiKey);
        }

        public Task<List<ApiKey>> GetApiKeys(string normalizedUsername)
        {
            return apiKeyDb.ApiKeys
                .Where(q => q.Owner == normalizedUsername)
                .ToListAsync()
                .ContinueWith(q => q.Result.Select(r => r.ToApiKey()).ToList());
        }

        public async Task DeleteApiKey(string normalizedUsername, string id)
        {
            var apiKey = await apiKeyDb.ApiKeys
                .FirstOrDefaultAsync(q => q.Owner == normalizedUsername && q.Id == id);

            if (apiKey == null)
                return;

            apiKeyDb.ApiKeys.Remove(apiKey);
            await apiKeyDb.SaveChangesAsync();
        }

        public async Task<Optional<(ApiKey ApiKey, byte[] EncryptionKey)>> ValidateToken(string token)
        {
            if (!await jweService.IsValid(token))
                return new();

            var claims = await jweService.GetClaims(token);
            var id = claims["id"];
            var encryptionKey = Convert.FromBase64String(claims["encryptionKey"]);
            var type = claims["type"];

            if (type != "apiToken")
                return new();

            var apiKey = await apiKeyDb.ApiKeys.FirstOrDefaultAsync(q => q.Id == id);
            if (apiKey == null)
                return new();


            return (apiKey.ToApiKey(), encryptionKey);
        }


    }
}
