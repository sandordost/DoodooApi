using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Doodoo.Modules.Todos
{
    public sealed class TodosDbContextFactory : IDesignTimeDbContextFactory<TodosDbContext>
    {
        public TodosDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<TodosDbContext>()
                .UseNpgsql("Host=localhost;Port=5067;Database=doodoodb;Username=doodoo;Password=doodoo",
                    npg => npg.MigrationsHistoryTable("__EFMigrationsHistory", TodosDbContext.Schema))
                .Options;

            return new TodosDbContext(options);
        }
    }
}
