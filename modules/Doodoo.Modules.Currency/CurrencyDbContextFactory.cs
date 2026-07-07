using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Doodoo.Modules.Currency
{
    // Design-time factory so `dotnet ef migrations add` works from this class library
    // without the web host. The connection string here is only used at design time.
    public sealed class CurrencyDbContextFactory : IDesignTimeDbContextFactory<CurrencyDbContext>
    {
        public CurrencyDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<CurrencyDbContext>()
                .UseNpgsql("Host=localhost;Port=5067;Database=doodoodb;Username=doodoo;Password=doodoo",
                    npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", CurrencyDbContext.Schema))
                .Options;

            return new CurrencyDbContext(options);
        }
    }
}
