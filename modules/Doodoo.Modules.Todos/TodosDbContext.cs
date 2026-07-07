using Doodoo.Modules.Todos.Entities;
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
                todo.HasIndex(t => t.ParentId);
                todo.HasIndex(t => new { t.OwnerId, t.ItemCategory, t.ParentId, t.Order });

                // Self-reference for saga trees. Hard-delete cascades to children; the app itself
                // uses soft-delete and cascades DeletedTimestamp recursively in the service.
                todo.HasMany(t => t.Children)
                    .WithOne(t => t.Parent)
                    .HasForeignKey(t => t.ParentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
