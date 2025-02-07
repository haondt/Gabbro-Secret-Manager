
namespace GabbroSecretManager.Api.Services
{
    public interface IApiSessionService
    {
        Task<(string NormalizedUsername, byte[] EncryptionKey)> GetUserDataAsync();
        Task<bool> IsAuthenticatedAsync();
    }
}