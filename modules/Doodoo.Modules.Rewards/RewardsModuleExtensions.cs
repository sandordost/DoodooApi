using DoodooApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Doodoo.Modules.Rewards
{
    /// <summary>Marker type so the host can reference this assembly for Wolverine discovery.</summary>
    public sealed class Anchor;

    public static class RewardsModuleExtensions
    {
        public static IServiceCollection AddRewardsModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<RewardsDbContext>(options =>
                options.UseNpgsql(connectionString, npg =>
                    npg.MigrationsHistoryTable("__EFMigrationsHistory", RewardsDbContext.Schema)));

            services.AddScoped<RewardService>();
            services.AddScoped<DifficultyRewardService>();

            return services;
        }
    }
}
