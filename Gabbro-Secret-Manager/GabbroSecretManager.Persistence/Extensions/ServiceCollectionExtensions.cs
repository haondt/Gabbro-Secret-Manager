using GabbroSecretManager.Persistence.Data;
using GabbroSecretManager.Persistence.Models;
using Haondt.Core.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GabbroSecretManager.Persistence.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGabbroSecretManagerPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GabbroSecretManagerPersistenceSettings>(configuration.GetSection(nameof(GabbroSecretManagerPersistenceSettings)));

            var settings = configuration.GetRequiredSection<GabbroSecretManagerPersistenceSettings>();
            var connectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = settings.DatabasePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Private,
                Pooling = true

            }.ToString();
            services.AddDbContext<ApplicationDbContext>(o =>
                o.UseSqlite(connectionString));
            services.AddDbContext<SecretsDbContext>(o =>
                o.UseSqlite(connectionString));
            services.AddDefaultIdentity<UserDataSurrogate>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            return services;
        }
    }
}
