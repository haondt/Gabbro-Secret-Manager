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

            services.AddScoped<PageRegistry>();
            var indexSettings = configuration.GetSection(nameof(IndexSettings)).Get<IndexSettings>();
            services.RegisterPage("navigationBar", "~/Core/Views/NavigationBar.cshtml", data =>
            {
                var mod =  new NavigationBarModel
                {
                    Pages = indexSettings!.NavigationBarPages.Select(p => (p, p.Equals(data.Form[NavigationBarModel.CurrentViewKey].ToString() ?? throw new InvalidOperationException(), StringComparison.OrdinalIgnoreCase))).ToList(),
                    Actions = []
                };
                return mod;
            }, false, false);
            services.RegisterPage("loader", "~/Core/Views/Loader.cshtml", () =>
            {
                throw new InvalidOperationException();
            }, false, false);
            services.RegisterPage("login", "~/Core/Views/Login.cshtml", () => new LoginModel(), true, false);
            services.RegisterPage("register", "~/Core/Views/Register.cshtml", () => new RegisterModel(), true, false);
            services.RegisterPage("index", "~/Core/Views/Index.cshtml", () => throw new InvalidOperationException(), false, false);

            services.AddSingleton<FileExtensionContentTypeProvider>();
            services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
            services.AddSingleton<CryptoService>();
            services.AddSingleton<UserService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<LifetimeHookService>();

            services.AddSingleton<IStorage, MemoryStorage>();
            services.Configure<PersistenceSettings>(configuration.GetSection(nameof(PersistenceSettings)));
            services.AddSingleton<IStorageService, StorageService>();

            return services;
        }

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IPageModel> modelFactory,
            bool setUrl = false,
            bool requiresAuthentication = true) => RegisterPage(services, page, viewPath, (_, _) => modelFactory(), setUrl, requiresAuthentication);

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IRequestData, IPageModel> modelFactory,
            bool setUrl = false,
            bool requiresAuthentication = true) => RegisterPage(services, page, viewPath, (_, data) => modelFactory(data), setUrl, requiresAuthentication);

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<PageRegistry, IRequestData, IPageModel> modelFactory,
            bool setUrl = false,
            bool requiresAuthentication = true)
        {
            services.AddScoped<IPageEntryFactory, PageEntryFactory>(_ => new PageEntryFactory
            {
                Page = page,
                SetUrl = setUrl ? page : null,
                ViewPath = viewPath,
                ModelFactory = modelFactory,
                RequiresAuthentication = requiresAuthentication
            });
            return services;
        }

    }
}
