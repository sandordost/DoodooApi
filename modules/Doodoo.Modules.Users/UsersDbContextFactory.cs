using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Doodoo.Modules.Users
{
    public sealed class UsersDbContextFactory : IDesignTimeDbContextFactory<UsersDbContext>
    {
        public UsersDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<UsersDbContext>()
                .UseNpgsql("Host=localhost;Port=5067;Database=doodoodb;Username=doodoo;Password=doodoo",
                    npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", UsersDbContext.Schema))
                .Options;

            return new UsersDbContext(options);
        }
    }
}
