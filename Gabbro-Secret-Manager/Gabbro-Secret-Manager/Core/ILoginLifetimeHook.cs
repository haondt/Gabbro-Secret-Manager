namespace Gabbro_Secret_Manager.Core
{
    public interface ILoginLifetimeHook : ILifetimeHook
    {
        public Task OnLoginAsync(string username, string password, string userKey, string sessionToken);
    }
}
