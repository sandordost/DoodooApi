namespace Doodoo.Modules.Inventory.Entities
{
    /// <summary>Per-user ownership of an <see cref="ItemDefinition"/>.</summary>
    public class InventoryEntry
    {
        public int Id { get; set; }

        // Logical reference to a Users-module AppUser. No DB FK / navigation across modules.
        public Guid OwnerId { get; set; }

        public int DefinitionId { get; set; }
        public ItemDefinition? Definition { get; set; }   // within-module navigation

        public int Quantity { get; set; } = 1;            // for stackable consumables
        public bool IsEquipped { get; set; }              // for customizations (max one per slot)

        public DateTime AcquiredAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedAtUtc { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }       // for memberships
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
