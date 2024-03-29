using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Persistence;
using Gabbro_Secret_Manager.Domain.Services;
using Gabbro_Secret_Manager.Views.Shared;

namespace Gabbro_Secret_Manager.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGabbroServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EncryptionKeyServiceSettings>(configuration.GetSection(nameof(EncryptionKeyServiceSettings)));
            services.AddSingleton<EncryptionKeyService>();
            services.AddSingleton<IGabbroStorageService, GabbroStorageService>();
            services.AddSingleton<IStorage>(sp => sp.GetRequiredService<IGabbroStorage>());
            services.AddSingleton<IGabbroStorage, FileGabbroStorage>();
            services.AddSingleton<IStorageService>(sp => sp.GetRequiredService<IGabbroStorageService>());
            services.AddSingleton<SecretService>();
            services.AddSingleton<UserDataService>();
            services.AddScoped<ILifetimeHook, LoginHook>();
            services.AddScoped<ILifetimeHook, RegisterHook>();
            

           return services;
        }

        public static IServiceCollection RegisterPages(this IServiceCollection services)
        {
            services.AddScoped<IPageEntryFactory, HomePageEntryFactory>();

            return services;
        }

    }
}
