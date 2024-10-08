﻿using Gabbro_Secret_Manager.Core;
using Gabbro_Secret_Manager.Core.Extensions;
using Gabbro_Secret_Manager.Core.Persistence;
using Gabbro_Secret_Manager.Domain.Filters;
using Gabbro_Secret_Manager.Domain.Models;
using Gabbro_Secret_Manager.Domain.Persistence;
using Gabbro_Secret_Manager.Domain.Services;
using Gabbro_Secret_Manager.Views.Shared;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;

namespace Gabbro_Secret_Manager.Domain
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGabbroServices(this IServiceCollection services, IConfiguration configuration)
        {
            // storage
            services.AddSingleton<IGabbroStorageService, GabbroStorageService>();
            services.AddSingleton<IStorage>(sp => sp.GetRequiredService<IGabbroStorage>());
            services.AddSingleton<IStorageService>(sp => sp.GetRequiredService<IGabbroStorageService>());


            var persistenceSettings = configuration.GetSection<Domain.Persistence.PersistenceSettings>();
            switch (persistenceSettings.Driver)
            {
                case GabbroPersistenceDrivers.Memory:
                    services.AddSingleton<IGabbroStorage, MemoryGabbroStorage>();
                    break;
                case GabbroPersistenceDrivers.File:
                    services.AddSingleton<IGabbroStorage, FileGabbroStorage>();
                    break;
                case GabbroPersistenceDrivers.MongoDb:
                    services.AddMongoDb(configuration);
                    break;
                case GabbroPersistenceDrivers.Postgres:
                    services.AddPostgreSQL(configuration);
                    break;
            }

            // services
            services.Configure<EncryptionKeyServiceSettings>(configuration.GetSection(nameof(EncryptionKeyServiceSettings)));
            services.AddSingleton<EncryptionKeyService>();
            services.AddSingleton<SecretService>();
            services.AddSingleton<UserDataService>();

            // lifetime hooks
            services.AddScoped<ILifetimeHook, LoginHook>();
            services.AddScoped<ILifetimeHook, RegisterHook>();

            // api keys
            services.Configure<JweSettings>(configuration.GetSection(nameof(JweSettings)));
            JweSettings.Validate(services.AddOptions<JweSettings>()).ValidateOnStart();
            services.AddSingleton<JweService>();
            services.AddSingleton<ApiKeyService>();
            services.AddScoped<ApiSessionService>();

            // views
            services.AddScoped<GabbroControllerHelper>();
            services.AddScoped<IControllerHelper>(sp => sp.GetRequiredService<GabbroControllerHelper>());
            services.AddScoped<IGabbroControllerHelper>(sp => sp.GetRequiredService<GabbroControllerHelper>());
            services.RegisterPages();

            // api filters
            services.AddScoped<ApiAuthenticationFilter>();
            services.AddScoped<ApiValidationFilter>();
            services.AddScoped<ApiErrorFilter>();

            return services;
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbSettings>(configuration.GetSection(nameof(MongoDbSettings)));
            services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);
                //clientSettings.ClusterConfigurator = cb =>
                //{
                //    cb.Subscribe<CommandStartedEvent>(e =>
                //    {
                //        Console.WriteLine(e.CommandName);
                //        Console.WriteLine(e.Command.ToJson());
                //    });
                //};
                return new MongoClient(clientSettings);
            });
            services.AddSingleton<IGabbroStorage, MongoDbGabbroStorage>();
            BsonSerializer.RegisterGenericSerializerDefinition(typeof(StorageKey<>), typeof(StorageKeyBsonConverter<>));
            BsonSerializer.RegisterSerializer(typeof(StorageKey), new StorageKeyBsonConverter());

            return services;
        }

        public static IServiceCollection AddPostgreSQL(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<PostgreSQLSettings>(configuration.GetSection(nameof(PostgreSQLSettings)));
            services.AddSingleton<IGabbroStorage, PostgreSQLGabbroStorage>();
            return services;
        }

        public static IServiceCollection RegisterPages(this IServiceCollection services)
        {
            services.AddScoped<GabbroPageRegistry>();
            services.AddScoped<IPageRegistry>(sp => sp.GetRequiredService<GabbroPageRegistry>());
            services.AddScoped<IGabbroPageRegistry>(sp => sp.GetRequiredService<GabbroPageRegistry>());

            services.AddScoped<IGabbroPageEntryFactory, HomePageEntryFactory>();
            services.AddScoped<IGabbroPageEntryFactory, UpsertSecretFormFactory>();
            services.RegisterPage("confirmDeleteSecretListEntry",
                "ConfirmDeleteSecretListEntry",
                data => new ConfirmDeleteSecretListEntryModel(data.Query.GetValue<Guid>(SecretListEntryModel.SecretIdKey)),
                false);
            services.AddScoped<IGabbroPageEntryFactory, SecretListEntryFactory>();
            services.AddScoped<IGabbroPageEntryFactory, SecretListFactory>();
            services.AddScoped<IPageEntryFactory, SettingsPageFactory>();
            services.RegisterPage("upsertSecretFormTag", "UpsertSecretFormTag", data => new UpsertSecretFormTagModel { Value = data.Query.GetValue<string>("value") });
            services.RegisterPage("confirmExportDataPrompt", "ConfirmExportDataPrompt", _ => new ConfirmExportDataPromptModel(), true, true);
            return services;
        }

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IPageRegistry, IRequestData, IPageModel> modelFactory,
            bool requiresAuthentication = true,
            bool requiresEncriptionKey = false,
            Func<HxHeaderBuilder, HxHeaderBuilder>? headerOptions = null)
        {
            services.AddScoped<IPageEntryFactory, PageEntryFactory>(_ => new GabbroPageEntryFactory
            {
                Page = page,
                ViewPath = viewPath,
                ModelFactory = modelFactory,
                RequiresAuthentication = requiresAuthentication,
                RequiresEncryptionKey = requiresEncriptionKey,
                ConfigureResponse = headerOptions
            });
            return services;
        }

        public static IServiceCollection RegisterPage(this IServiceCollection services,
            string page,
            string viewPath,
            Func<IRequestData, IPageModel> modelFactory,
            bool requiresAuthentication = true,
            bool requiresEncriptionKey = false,
            Func<HxHeaderBuilder, HxHeaderBuilder>? headerOptions = null) => RegisterPage(services, page, viewPath, (_, data) => modelFactory(data), requiresAuthentication, requiresEncriptionKey, headerOptions);
    }
}
