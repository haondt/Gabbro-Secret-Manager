using GabbroSecretManager.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GabbroSecretManager.Persistence.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static void PerformDatabaseMigrations(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.Migrate();

            var db2 = scope.ServiceProvider.GetRequiredService<SecretsDbContext>();
            db2.Database.Migrate();

            var db3 = scope.ServiceProvider.GetRequiredService<ApiKeyDbContext>();
            db3.Database.Migrate();
        }
    }
}
