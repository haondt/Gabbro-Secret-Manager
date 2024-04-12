using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Services;

namespace Gabbro_Secret_Manager.Domain
{
    public class LoginHook(UserDataService userDataService, EncryptionKeyService encryptionKeyService) : ILoginLifetimeHook
    {
        public async Task OnLoginAsync(string username, string password, StorageKey userKey, string sessionToken)
        {
            var userData = await userDataService.GetUserData(userKey);
            encryptionKeyService.GetOrCreateEncryptionKey(sessionToken, userKey, password, userData.EncryptionKeySettings);
        }
    }
}
