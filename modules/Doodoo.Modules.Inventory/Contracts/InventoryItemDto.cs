using Doodoo.Modules.Inventory.Enums;

namespace Doodoo.Modules.Inventory.Contracts
{
    public sealed record InventoryItemDto(
        int EntryId,
        int DefinitionId,
        string Key,
        string Name,
        string? Description,
        ItemKind Kind,
        ItemSlot Slot,
        string? Category,
        ContentType ContentType,
        string? Content,
        ConsumableEffect Effect,
        int EffectAmount,
        int Quantity,
        bool IsEquipped,
        DateTime? ExpiresAtUtc);
}
