using DoodooApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Doodoo.Modules.Todos
{
    /// <summary>Marker type so the host can reference this assembly for Wolverine discovery.</summary>
    public sealed class Anchor;

    public static class TodosModuleExtensions
    {
        public static IServiceCollection AddTodosModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<TodosDbContext>(options =>
                options.UseNpgsql(connectionString, npg =>
                    npg.MigrationsHistoryTable("__EFMigrationsHistory", TodosDbContext.Schema)));

            services.AddScoped<TodoItemService>();

            return services;
        }
    }
}
