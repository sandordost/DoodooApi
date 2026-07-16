using Doodoo.Modules.Inventory.Entities;
using Doodoo.Modules.Inventory.Enums;
using Microsoft.EntityFrameworkCore;

namespace Doodoo.Modules.Inventory
{
    public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
    {
        public const string Schema = "inventory";

        public DbSet<ItemDefinition> ItemDefinitions { get; set; }
        public DbSet<InventoryEntry> InventoryEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<ItemDefinition>(def =>
            {
                def.HasKey(d => d.Id);
                def.HasIndex(d => d.Key).IsUnique();
                // Content is heterogeneous (JSON skin-spec, plain asset token, sanitized svg/html),
                // so it is stored as text; JSON validity is enforced server-side per ContentType.
                def.Property(d => d.EffectAmount).HasDefaultValue(0);
            });

            modelBuilder.Entity<InventoryEntry>(entry =>
            {
                entry.HasKey(e => e.Id);

                entry.HasOne(e => e.Definition)
                    .WithMany()
                    .HasForeignKey(e => e.DefinitionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entry.HasIndex(e => e.OwnerId);

                // A user owns at most one entry per definition (stackables use Quantity).
                entry.HasIndex(e => new { e.OwnerId, e.DefinitionId }).IsUnique();
            });

            SeedDefinitions(modelBuilder);
        }

        // Baked into InitialCreate. Fixed ids so re-running the migration is deterministic.
        // Defaults (IsDefault=true) are granted to every user on registration.
        private static void SeedDefinitions(ModelBuilder modelBuilder)
        {
            const string starrySpec =
                "{\"version\":1,\"slot\":\"AppBackground\",\"layers\":[" +
                "{\"type\":\"gradient\",\"gradient\":{\"from\":\"#0f172a\",\"to\":\"#1e1b4b\",\"angle\":135},\"position\":\"cover\",\"zIndex\":0}," +
                "{\"type\":\"image\",\"url\":\"https://assets.doo-doo.nl/backgrounds/stars.png\",\"position\":\"cover\",\"opacity\":0.6,\"zIndex\":1,\"animation\":{\"preset\":\"float\",\"durationMs\":8000}}" +
                "]}";

            const string neonNavTokens =
                "{\"version\":1,\"slot\":\"NavButtons\",\"layers\":[]," +
                "\"tokens\":{\"--nav-btn-bg\":\"#1e1b4b\",\"--nav-btn-fg\":\"#e0e7ff\",\"--nav-btn-radius\":\"16px\"}}";

            modelBuilder.Entity<ItemDefinition>().HasData(
                new ItemDefinition
                {
                    Id = 1, Key = "default-confetti", Name = "Confetti", Description = "De standaard voltooiings-animatie.",
                    Kind = ItemKind.Customization, Slot = ItemSlot.CompletionAnimation, Category = "Animaties",
                    ContentType = ContentType.AssetToken, Content = "confetti", IsDefault = true, IsActive = true
                },
                new ItemDefinition
                {
                    Id = 2, Key = "default-app-background", Name = "Standaard achtergrond",
                    Kind = ItemKind.Customization, Slot = ItemSlot.AppBackground, Category = "Achtergrond",
                    ContentType = ContentType.AssetToken, Content = "default", IsDefault = true, IsActive = true
                },
                new ItemDefinition
                {
                    Id = 3, Key = "starry-night", Name = "Sterrennacht", Description = "Een geanimeerde sterrenhemel-achtergrond.",
                    Kind = ItemKind.Customization, Slot = ItemSlot.AppBackground, Category = "Achtergrond",
                    ContentType = ContentType.Spec, Content = starrySpec, IsDefault = false, IsActive = true
                },
                new ItemDefinition
                {
                    Id = 4, Key = "default-todo-card", Name = "Standaard todo-kaart",
                    Kind = ItemKind.Customization, Slot = ItemSlot.TodoCardBackground, Category = "Todo's",
                    ContentType = ContentType.AssetToken, Content = "default", IsDefault = true, IsActive = true
                },
                new ItemDefinition
                {
                    Id = 5, Key = "default-nav-buttons", Name = "Standaard knoppen",
                    Kind = ItemKind.Customization, Slot = ItemSlot.NavButtons, Category = "Buttons",
                    ContentType = ContentType.AssetToken, Content = "default", IsDefault = true, IsActive = true
                },
                new ItemDefinition
                {
                    Id = 6, Key = "neon-nav-buttons", Name = "Neon knoppen", Description = "Herskin de navigatieknoppen (paars/neon).",
                    Kind = ItemKind.Customization, Slot = ItemSlot.NavButtons, Category = "Buttons",
                    ContentType = ContentType.Spec, Content = neonNavTokens, IsDefault = false, IsActive = true
                },
                new ItemDefinition
                {
                    Id = 7, Key = "default-profile-button", Name = "Standaard profielknop",
                    Kind = ItemKind.Customization, Slot = ItemSlot.ProfileButton, Category = "Profiel",
                    ContentType = ContentType.AssetToken, Content = "default", IsDefault = true, IsActive = true
                },
                new ItemDefinition
                {
                    Id = 8, Key = "bag-of-coins", Name = "Zakje goud", Description = "Levert 20 goud op bij gebruik.",
                    Kind = ItemKind.Consumable, Effect = ConsumableEffect.GrantGold, EffectAmount = 20,
                    Stackable = true, IsDefault = false, IsActive = true
                },
                new ItemDefinition
                {
                    Id = 9, Key = "pro-membership", Name = "Pro membership", Description = "Ontgrendelt pro-functies.",
                    Kind = ItemKind.Membership, IsDefault = false, IsActive = true
                }
            );
        }
    }
}
