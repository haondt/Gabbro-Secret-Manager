namespace Gabbro_Secret_Manager.Core
{
    public interface ISessionService
    {
        public Task<bool> IsAuthenticatedAsync();
        public Task<string> GetUserKeyAsync();
        public void Reset(string? sessionToken = null);
        public string? SessionToken { get; }
    }
}
