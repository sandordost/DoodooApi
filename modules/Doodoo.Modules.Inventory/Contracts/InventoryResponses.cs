using Doodoo.Modules.Inventory.Enums;

namespace Doodoo.Modules.Inventory.Contracts
{
    /// <summary>Everything the frontend needs on startup, in one response.</summary>
    public sealed record InventoryResponse(
        IReadOnlyList<InventoryItemDto> Items,
        IReadOnlyList<EquippedCustomizationDto> Equipped,
        bool IsPro,
        string Version);

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

    /// <summary>The payload of a currently equipped customization, one per occupied slot.</summary>
    public sealed record EquippedCustomizationDto(
        ItemSlot Slot,
        int EntryId,
        int DefinitionId,
        string Key,
        ContentType ContentType,
        string? Content);

    public sealed record UseItemResponse(
        int EntryId,
        int RemainingQuantity,
        ConsumableEffect Effect,
        int EffectAmount,
        decimal? NewGoldBalance,
        int? NewSapphireBalance);
}
