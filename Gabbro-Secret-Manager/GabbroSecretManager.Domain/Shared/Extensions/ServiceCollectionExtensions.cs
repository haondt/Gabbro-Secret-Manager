﻿using GabbroSecretManager.Domain.Authentication.Services;
using GabbroSecretManager.Domain.Cryptography.Models;
using GabbroSecretManager.Domain.Cryptography.Services;
using GabbroSecretManager.Domain.Secrets.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GabbroSecretManager.Domain.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGabbroSecretManagerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<ISecretService, SecretService>();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/authentication/login";
            });

            services.AddSingleton<IEncryptionKeyCacheService, EncryptionKeyCacheService>();
            services.Configure<EncryptionKeyCacheSettings>(configuration.GetSection(nameof(EncryptionKeyCacheSettings)));

            return services;
        }
    }
}
