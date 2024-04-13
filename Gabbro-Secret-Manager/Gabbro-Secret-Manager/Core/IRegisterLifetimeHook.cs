using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public interface IRegisterLifetimeHook : ILifetimeHook
    {
        public Task OnRegisterAsync(User user, StorageKey<User> userKey);
    }
}
