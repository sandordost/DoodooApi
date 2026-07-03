using DoodooApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Doodoo.Modules.Currency
{
    /// <summary>Marker type so the host can reference this assembly for Wolverine discovery.</summary>
    public sealed class Anchor;

    public static class CurrencyModuleExtensions
    {
        public static IServiceCollection AddCurrencyModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<CurrencyDbContext>(options =>
                options.UseNpgsql(connectionString, npg =>
                    npg.MigrationsHistoryTable("__EFMigrationsHistory", CurrencyDbContext.Schema)));

            services.AddScoped<TransactionService>();
            services.AddScoped<CurrencyAccountService>();

            return services;
        }
    }
}
