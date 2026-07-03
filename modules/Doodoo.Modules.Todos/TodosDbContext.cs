using DoodooApi.Models.Main.TodoItems;
using Microsoft.EntityFrameworkCore;

namespace Doodoo.Modules.Todos
{
    public sealed class TodosDbContext(DbContextOptions<TodosDbContext> options) : DbContext(options)
    {
        public const string Schema = "todos";

        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<TodoItem>(todo =>
            {
                todo.HasKey(t => t.Id);
                todo.HasIndex(t => t.OwnerId);
            });
        }
    }
}
