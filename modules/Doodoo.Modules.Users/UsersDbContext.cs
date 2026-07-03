using DoodooApi.Models.Main.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Doodoo.Modules.Users
{
    public sealed class UsersDbContext(DbContextOptions<UsersDbContext> options)
        : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options)
    {
        public const string Schema = "users";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<AppUser>(user =>
            {
                user.HasIndex(u => u.UserName).IsUnique();
                user.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
