using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterPages(this IServiceCollection services)
        {
            services.RegisterPage("home", "Home", _ => new HomeModel());
            services.RegisterPage("login", "Login", _ => new LoginModel(), false);
            services.RegisterPage("register", "Register", _ => new RegisterModel(), false);

            return services;
        }
        public static IServiceCollection RegisterPartialPages(this IServiceCollection services)
        {
            services.RegisterPartialPage("home", "Home", _ => new HomeModel());
            services.RegisterPartialPage("login", "Login", _ => new LoginModel(), false);
            services.RegisterPartialPage("register", "Register", _ => new RegisterModel(), false);
            return services;
        }
    }
}
