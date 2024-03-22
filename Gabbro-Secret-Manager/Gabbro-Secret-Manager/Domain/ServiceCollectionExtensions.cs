using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterPages(this IServiceCollection services)
        {
            services.RegisterPage("home", "Home", _ => new HomeModel());
            services.RegisterPage("login", "Login", _ => new LoginModel());
            return services;
        }
    }
}
