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
            services.RegisterPartialPage("navigationbar", "~/Core/Views/NavigationBar.cshtml", query =>
            {
                return new NavigationBarModel { CurrentPage = (string)query["current"]! };
            });

            services.AddSingleton<FileExtensionContentTypeProvider>();

            return services;
        }

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IReadOnlyDictionary<string, object?>, object> modelFactory)
        { 
            services.AddSingleton(new PageRegistryEntry
            {
                Type = PageEntryType.Page,
                Page = page,
                ViewPath = viewPath,
                ModelFactory = modelFactory
            });
            return services;
        }

        public static IServiceCollection RegisterPartialPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IReadOnlyDictionary<string, object?>, object> modelFactory)
        {
            services.AddSingleton(new PageRegistryEntry
            {
                Type = PageEntryType.Page,
                Page = page,
                ViewPath = viewPath,
                ModelFactory = modelFactory
            });
            return services;
        }
    }
}
