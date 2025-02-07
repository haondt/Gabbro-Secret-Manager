using GabbroSecretManager.Api.Filters;
using GabbroSecretManager.Api.Services;

namespace GabbroSecretManager.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGabbroSecretManagerApi(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IApiSessionService, ApiSessionService>();

            services.AddScoped<ApiAuthenticationFilter>();
            services.AddScoped<ApiValidationFilter>();
            services.AddScoped<ApiErrorFilter>();

            return services;
        }
    }
}
