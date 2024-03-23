using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.StaticFiles;

namespace Gabbro_Secret_Manager.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AssetsSettings>(configuration.GetSection(nameof(AssetsSettings)));
            services.AddSingleton<AssetProvider>();
            services.Configure<ColorSettings>(configuration.GetSection(nameof(ColorSettings)));
            services.AddSingleton<StylesProvider>();
            services.Configure<IndexSettings>(configuration.GetSection(nameof(IndexSettings)));

            services.AddSingleton<PageRegistry>();
            var indexSettings = configuration.GetSection(nameof(IndexSettings)).Get<IndexSettings>();
            services.RegisterPartialPage("navigationbar", "~/Core/Views/NavigationBar.cshtml", data =>
            {
                return new NavigationBarModel
                {
                    CurrentPage = data.Query["current"].ToString() ?? throw new InvalidOperationException(),
                    Pages = indexSettings!.NavigationBarPages
                };
            }, false);

            services.AddSingleton<FileExtensionContentTypeProvider>();
            services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
            services.AddSingleton<CryptoService>();
            services.AddSingleton<UserService>();

            services.AddSingleton<IStorage, MemoryStorage>();
            services.Configure<PersistenceSettings>(configuration.GetSection(nameof(PersistenceSettings)));
            services.AddSingleton<IStorageService, StorageService>();

            return services;
        }

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IRequestData, object> modelFactory,
            bool requiresAuthentication = true)
        { 
            services.AddSingleton(new PageRegistryEntry
            {
                Type = PageEntryType.Page,
                Page = page,
                ViewPath = viewPath,
                ModelFactory = modelFactory,
                RequiresAuthentication = requiresAuthentication
            });
            return services;
        }

        public static IServiceCollection RegisterPartialPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IRequestData, object> modelFactory,
            bool requiresAuthentication = true)
        {
            services.AddSingleton(new PageRegistryEntry
            {
                Type = PageEntryType.Partial,
                Page = page,
                ViewPath = viewPath,
                ModelFactory = modelFactory,
                RequiresAuthentication = requiresAuthentication
            });
            return services;
        }
    }
}
