using GabbroSecretManager.Core.Models;
using Haondt.Core.Models;

namespace GabbroSecretManager.Domain.Api.Services
{
    public interface IApiKeyService
    {
        Task<(string Token, ApiKey ApiKey)> CreateApiKey(string name, string normalizedUsername, string password, byte[] encryptionKey);
        Task DeleteApiKey(string normalizedUsername, string id);
        Task<List<ApiKey>> GetApiKeys(string normalizedUsername);
        Task<Optional<(ApiKey ApiKey, byte[] EncryptionKey)>> ValidateToken(string token);
    }
}
