using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;
using Gabbro_Secret_Manager.Domain.Services;
using Gabbro_Secret_Manager.Views.Shared;
using Microsoft.Extensions.Options;

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

            services.AddSingleton<ApiKeyService>();
            services.Configure<JweSettings>(configuration.GetSection(nameof(JweSettings)));
            JweSettings.Validate(services.AddOptions<JweSettings>()).ValidateOnStart();
            services.AddSingleton<JweService>();

            return services;
        }

        public static IServiceCollection RegisterPages(this IServiceCollection services)
        {
            services.AddScoped<IPageEntryFactory, HomePageEntryFactory>();
            services.AddScoped<IPageEntryFactory, PageEntryFactory>(_ => new PageEntryFactory
            {
                Page = "passwordReentryForm",
                SetUrl = "",
                ViewPath = "PasswordReentryForm",
                ModelFactory = (_, _) => new PasswordReentryFormModel(),
                RequiresAuthentication = false
            });
            services.AddScoped<IPageEntryFactory, PageEntryFactory>(_ => new PageEntryFactory
            {
                Page = "passwordReentryModal",
                SetUrl = "",
                ViewPath = "PasswordReentryModal",
                ModelFactory = (_, _) => new PasswordReentryModalModel(),
                RequiresAuthentication = false
            });
            services.AddScoped<IPageEntryFactory, UpsertSecretFormFactory>();
            services.RegisterPage("confirmDeleteSecretListEntry",
                "ConfirmDeleteSecretListEntry",
                data => new ConfirmDeleteSecretListEntryModel(data.Query.GetValue<string>(SecretListEntryModel.SecretNameKey)),
                false, false);
            services.AddScoped<IPageEntryFactory, SecretListEntryFactory>();
            services.AddScoped<IPageEntryFactory, SecretListFactory>();
            services.AddScoped<IPageEntryFactory, SettingsPageFactory>();
            services.RegisterPage("upsertSecretFormTag", "UpsertSecretFormTag", data => new UpsertSecretFormTagModel { Value = data.Query.GetValue<string>("value") });
            return services;
        }
    }
}
