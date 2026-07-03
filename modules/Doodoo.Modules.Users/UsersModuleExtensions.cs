using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Doodoo.Modules.Users
{
    /// <summary>Marker type so the host can reference this assembly for Wolverine discovery.</summary>
    public sealed class Anchor;

    public static class UsersModuleExtensions
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<UsersDbContext>(options =>
                options.UseNpgsql(connectionString, npg =>
                    npg.MigrationsHistoryTable("__EFMigrationsHistory", UsersDbContext.Schema)));

            return services;
        }
    }
}
