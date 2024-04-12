using Gabbro_Secret_Manager.Core.Persistence;

namespace Gabbro_Secret_Manager.Core
{
    public interface ISessionService
    {
        public Task<bool> IsAuthenticatedAsync();
        public Task<StorageKey> GetUserKeyAsync();
        public void Reset(string? sessionToken = null);
        public string? SessionToken { get; }
    }
}
