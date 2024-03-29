using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Domain.Services;

namespace Gabbro_Secret_Manager.Domain
{
    public class RegisterHook(UserDataService userDataService) : IRegisterLifetimeHook
    {
        public Task OnRegisterAsync(User user, string userKey)
        {
            return userDataService.InitializeUserData(userKey);
        }
    }
}
