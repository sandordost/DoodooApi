using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Doodoo.Modules.Inventory
{
    // Design-time factory so `dotnet ef migrations add` works from this class library
    // without the web host.
    public sealed class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
    {
        public InventoryDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("DOODOO_DB")
                ?? throw new InvalidOperationException(
                    "Set DOODOO_DB to run design-time migrations for InventoryDbContext.");

            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseNpgsql(connectionString,
                    npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", InventoryDbContext.Schema))
                .Options;

            return new InventoryDbContext(options);
        }
    }
}
