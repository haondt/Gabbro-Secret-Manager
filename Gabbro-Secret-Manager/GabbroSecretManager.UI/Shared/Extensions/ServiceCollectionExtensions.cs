using GabbroSecretManager.UI.Shared.Middlewares;
using GabbroSecretManager.UI.Shared.Models;
using GabbroSecretManager.UI.Shared.Services;
using Haondt.Web.Assets;
using Haondt.Web.BulmaCSS.Extensions;
using Haondt.Web.Core.Services;
using Haondt.Web.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ComponentFactory = GabbroSecretManager.UI.Shared.Services.ComponentFactory;

namespace GabbroSecretManager.UI.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGabbroSecretManagerUI(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ILayoutComponentFactory, LayoutComponentFactory>();
            services.AddBulmaCSSHeadEntries();
            services.AddBulmaCSSAssetSources();
            services.AddGabbroSecretManagerHeadEntries();
            services.Configure<HostSettings>(configuration.GetSection(nameof(HostSettings)));
            services.AddSingleton<IComponentFactory, ComponentFactory>();
            services.AddScoped<IUISessionService, UISessionService>();

            services.AddSingleton<ISpecificExceptionActionResultFactory, PageExceptionActionResultFactory>();
            services.AddSingleton<ISpecificExceptionActionResultFactory, ToastExceptionActionResultFactory>();
            services.AddSingleton<IExceptionActionResultFactory, ExceptionActionResultFactoryDelegator>();

            services.AddScoped<ModelStateValidationFilter>();


            return services;
        }

        public static IServiceCollection AddGabbroSecretManagerHeadEntries(this IServiceCollection services)
        {

            var assembly = typeof(ServiceCollectionExtensions).Assembly;
            var assemblyPrefix = assembly.GetName().Name;

            services.AddSingleton<IAssetSource>(sp => new ManifestAssetSource(assembly));


            services.AddScoped<IHeadEntryDescriptor>(sp => new ScriptDescriptor
            {
                Uri = "https://kit.fontawesome.com/afd44816da.js",
                CrossOrigin = "anonymous"
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new TitleDescriptor
            {
                Title = "GSM",
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new MetaDescriptor
            {
                Name = "htmx-config",
                Content = @"{
                    ""responseHandling"": [
                        { ""code"": ""204"", ""swap"": false },
                        { ""code"": "".*"", ""swap"": true }
                    ]
                }",
            });
            services.AddScoped<IHeadEntryDescriptor>(_ => new StyleSheetDescriptor
            {
                Uri = "/_asset/GabbroSecretManager.UI.wwwroot.app.css"
            });
            return services;
        }
    }
}
