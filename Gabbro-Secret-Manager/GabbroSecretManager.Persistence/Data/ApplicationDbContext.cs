using GabbroSecretManager.Persistence.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GabbroSecretManager.Persistence.Data
{
    public class ApplicationDbContext : IdentityDbContext<UserDataSurrogate>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}
