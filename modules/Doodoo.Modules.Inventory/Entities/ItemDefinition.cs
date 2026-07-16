using Doodoo.Modules.Inventory.Enums;

namespace Doodoo.Modules.Inventory.Entities
{
    /// <summary>
    /// Catalog entry (system/admin-managed). Holds the generated customization payload. Not per-user.
    /// </summary>
    public class ItemDefinition
    {
        public int Id { get; set; }
        public required string Key { get; set; }          // unique slug, e.g. "starry-background"
        public required string Name { get; set; }
        public string? Description { get; set; }

        public ItemKind Kind { get; set; }

        // Customization only:
        public ItemSlot Slot { get; set; } = ItemSlot.None;
        public string? Category { get; set; }             // UI grouping (e.g. "Achtergrond")
        public ContentType ContentType { get; set; } = ContentType.None;
        public string? Content { get; set; }              // JSON spec / token / sanitized svg|html

        // Consumable only:
        public ConsumableEffect Effect { get; set; } = ConsumableEffect.None;
        public int EffectAmount { get; set; }

        public bool Stackable { get; set; }
        public int? UnlockAtLevel { get; set; }           // hook for the future Level system
        public bool IsDefault { get; set; }               // granted to every user on registration
        public bool IsActive { get; set; } = true;
    }
}
