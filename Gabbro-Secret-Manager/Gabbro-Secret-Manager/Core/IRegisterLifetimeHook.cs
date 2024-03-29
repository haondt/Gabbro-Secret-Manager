namespace Gabbro_Secret_Manager.Core
{
    public interface IRegisterLifetimeHook : ILifetimeHook
    {
        public Task OnRegisterAsync(User user, string userKey);
    }
}
