using GabbroSecretManager.Domain.Authentication.Models;

namespace GabbroSecretManager.Domain.Authentication.Services
{
    public interface IUserService
    {
        Task SignOutAsync();
        Task<RefreshEncryptionKeyResult> TryRefreshEncryptionKey(string password);
        public Task<RegisterUserResult> TryRegister(string username, string password);
        public Task<AuthenticateUserResult> TrySignIn(string username, string password);
    }
}
