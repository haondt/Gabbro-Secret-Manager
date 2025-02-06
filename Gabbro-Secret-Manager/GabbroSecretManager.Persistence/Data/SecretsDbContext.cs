using GabbroSecretManager.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace GabbroSecretManager.Persistence.Data
{
    public class SecretsDbContext(DbContextOptions<SecretsDbContext> options) : DbContext(options)
    {
        public DbSet<SecretSurrogate> Secrets { get; set; }
        public DbSet<TagSurrogate> Tags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SecretSurrogate>()
                .ToTable("Secrets")
                .HasKey(s => s.Id);

            modelBuilder.Entity<SecretSurrogate>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<TagSurrogate>()
                .ToTable("Tags")
                .HasKey(t => new { t.SecretId, t.Tag });

            modelBuilder.Entity<TagSurrogate>()
                .HasOne(s => s.Secret)
                .WithMany(s => s.Tags)
                .HasForeignKey(s => s.SecretId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
