using Haondt.Core.Models;

namespace GabbroSecretManager.Domain.Authentication.Services
{
    public interface ISessionService
    {
        Task<Optional<string>> GetNormalizedUsernameAsync();
    }
}
