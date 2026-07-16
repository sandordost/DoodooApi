using System.ComponentModel.DataAnnotations;
using Doodoo.Modules.Inventory.Enums;

namespace Doodoo.Modules.Inventory.Contracts
{
    public sealed class CreateItemDefinitionRequest
    {
        [Required] public string Key { get; set; } = string.Empty;
        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ItemKind Kind { get; set; }

        public ItemSlot Slot { get; set; } = ItemSlot.None;
        public string? Category { get; set; }
        public ContentType ContentType { get; set; } = ContentType.None;
        public string? Content { get; set; }

        public ConsumableEffect Effect { get; set; } = ConsumableEffect.None;
        public int EffectAmount { get; set; }

        public bool Stackable { get; set; }
        public int? UnlockAtLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public sealed class UpdateItemDefinitionRequest
    {
        [Required] public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ItemSlot Slot { get; set; } = ItemSlot.None;
        public string? Category { get; set; }
        public ContentType ContentType { get; set; } = ContentType.None;
        public string? Content { get; set; }
        public ConsumableEffect Effect { get; set; } = ConsumableEffect.None;
        public int EffectAmount { get; set; }
        public bool Stackable { get; set; }
        public int? UnlockAtLevel { get; set; }
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Grant an item to one user, or to everyone when <see cref="All"/> is true.</summary>
    public sealed class GrantItemRequest
    {
        public Guid? UserId { get; set; }
        public bool All { get; set; }
        [Required] public string DefinitionKey { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
    }
}
