﻿namespace Gabbro_Secret_Manager.Core
{
    public class LifetimeHookService(IEnumerable<ILifetimeHook> hooks)
    {
        private Task OnEventAsync<T>(Func<T, Task> func) =>
            Task.WhenAll(hooks
                .Where(h => h is T)
                .Cast<T>()
                .Select(func));

        public Task OnLoginAsync(string username, string password, string userKey, string sessionToken) =>
            OnEventAsync<ILoginLifetimeHook>(h => h.OnLoginAsync(username, password, userKey, sessionToken));

        public Task OnRegisterAsync(User user, string userKey) =>
            OnEventAsync<IRegisterLifetimeHook>(h => h.OnRegisterAsync(user, userKey));
    }
}
