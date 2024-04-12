using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public interface ILoginLifetimeHook : ILifetimeHook
    {
        public Task OnLoginAsync(string username, string password, StorageKey userKey, string sessionToken);
    }
}
