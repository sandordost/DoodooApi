using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Doodoo.Modules.Rewards
{
    public sealed class RewardsDbContextFactory : IDesignTimeDbContextFactory<RewardsDbContext>
    {
        public RewardsDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<RewardsDbContext>()
                .UseNpgsql("Host=localhost;Port=5067;Database=doodoodb;Username=doodoo;Password=doodoo",
                    npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", RewardsDbContext.Schema))
                .Options;

            return new RewardsDbContext(options);
        }
    }
}
