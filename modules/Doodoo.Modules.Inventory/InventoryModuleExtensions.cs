using Doodoo.Modules.Inventory.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Doodoo.Modules.Inventory
{
    public static class InventoryModuleExtensions
    {
        public static IServiceCollection AddInventoryModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<InventoryDbContext>(options =>
                options.UseNpgsql(connectionString, npg =>
                    npg.MigrationsHistoryTable("__EFMigrationsHistory", InventoryDbContext.Schema)));

            services.AddScoped<InventoryService>();

            return services;
        }
    }
}
