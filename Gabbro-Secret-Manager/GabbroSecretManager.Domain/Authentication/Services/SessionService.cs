using GabbroSecretManager.Persistence.Models;
using Haondt.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace GabbroSecretManager.Domain.Authentication.Services
{
    internal class SessionService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<UserDataSurrogate> userManager
        ) : ISessionService
    {
        public bool IsAuthenticated
        {
            get
            {
                return httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;
            }
        }

        public async Task<Optional<string>> GetNormalizedUsernameAsync()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null)
                return new Optional<string>();

            var surrogate = await userManager.GetUserAsync(user);
            if (surrogate == null)
                return new Optional<string>();

            var userData = surrogate.ToUserData();
            return userData.NormalizedUsername;
        }
    }
}
