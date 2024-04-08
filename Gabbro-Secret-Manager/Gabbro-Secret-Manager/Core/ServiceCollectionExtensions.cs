using Gabbro_Secret_Manager.Core.DynamicForm;
using Gabbro_Secret_Manager.Core.DynamicForms;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Core.Views;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;

namespace Gabbro_Secret_Manager.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IMemoryCache, MemoryCache>();
            services.Configure<AssetsSettings>(configuration.GetSection(nameof(AssetsSettings)));
            services.AddSingleton<AssetProvider>();
            services.Configure<ColorSettings>(configuration.GetSection(nameof(ColorSettings)));
            services.AddSingleton<StylesProvider>();
            services.Configure<IndexSettings>(configuration.GetSection(nameof(IndexSettings)));

            services.AddScoped<IPageRegistry, PageRegistry>();

            var indexSettings = configuration.GetSection(nameof(IndexSettings)).Get<IndexSettings>();
            services.RegisterPage("navigationBar", "~/Core/Views/NavigationBar.cshtml", data =>
            {
                data.Query.TryGetValue(NavigationBarModel.CurrentViewKey, out string? castedValue);
                return new NavigationBarModel
                {
                    Pages = indexSettings!.NavigationBarPages.Select(p => (p, p.Equals(castedValue, StringComparison.OrdinalIgnoreCase))).ToList(),
                    Actions = []
                };
            }, false);
            services.RegisterPage("loader", "~/Core/Views/Loader.cshtml", () => throw new InvalidOperationException(), false);
            services.RegisterPage("dynamicForm", "~/Core/Views/DynamicForm.cshtml", () => throw new InvalidOperationException(), false);
            services.RegisterPage("dynamicFormWithAuthentication", "~/Core/Views/DynamicForm.cshtml", () => throw new InvalidOperationException(), true);
            //services.RegisterPage("login", "~/Core/Views/Login.cshtml", () => new LoginModel(), false, r => r.ConfigureForPage("login"));
            services.RegisterPage("login", "~/Core/Views/DynamicForm.cshtml", new LoginDynamicFormFactory("").Create, false, r => r.ConfigureForPage("login"));
            services.RegisterPage("register", "~/Core/Views/Register.cshtml", () => new RegisterModel(), false, r => r.ConfigureForPage("register"));
            services.RegisterPage("index", "~/Core/Views/Index.cshtml", () => throw new InvalidOperationException(), false);
            services.RegisterPage("toast", "~/Core/Views/Toast.cshtml", () => throw new InvalidOperationException(), false,
                r => r.ReSwap("afterbegin").ReTarget("#toast-container"));
            services.RegisterPage("modal", "~/Core/Views/Modal.cshtml", () => throw new InvalidOperationException(), false, r =>
                r.ReSwap("innerHTML").ReTarget("#modal-container"));
                

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

            services.AddScoped<ToastResponseService>();
            services.AddScoped<ToastErrorFilter>();
            services.AddScoped<ValidationFilter>();

            services.AddScoped<IControllerHelper, ControllerHelper>();

            return services;
        }

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IPageModel> modelFactory,
            bool requiresAuthentication = true,
            Func<HxHeaderBuilder, HxHeaderBuilder>? headerOptions = null) => RegisterPage(services, page, viewPath, (_, _) => modelFactory(), requiresAuthentication, headerOptions);

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IRequestData, IPageModel> modelFactory,
            bool requiresAuthentication = true,
            Func<HxHeaderBuilder, HxHeaderBuilder>? headerOptions = null) => RegisterPage(services, page, viewPath, (_, data) => modelFactory(data), requiresAuthentication, headerOptions);

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IPageRegistry, IRequestData, IPageModel> modelFactory,
            bool requiresAuthentication = true,
            Func<HxHeaderBuilder, HxHeaderBuilder>? headerOptions = null)
        {
            services.AddScoped<IPageEntryFactory, PageEntryFactory>(_ => new PageEntryFactory
            {
                Page = page,
                ViewPath = viewPath,
                ModelFactory = modelFactory,
                RequiresAuthentication = requiresAuthentication,
                ConfigureResponse = headerOptions
            });
            return services;
        }

    }
}
