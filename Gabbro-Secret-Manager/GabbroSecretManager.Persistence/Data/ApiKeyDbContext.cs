using GabbroSecretManager.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace GabbroSecretManager.Persistence.Data
{
    public class ApiKeyDbContext(DbContextOptions<ApiKeyDbContext> options) : DbContext(options)
    {
        public DbSet<ApiKeySurrogate> ApiKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApiKeySurrogate>()
                .ToTable("ApiKeys")
                .HasKey(s => s.Id);
        }
    }
}
