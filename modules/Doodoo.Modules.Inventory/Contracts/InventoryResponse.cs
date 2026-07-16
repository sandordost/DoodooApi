namespace Doodoo.Modules.Inventory.Contracts
{
    /// <summary>Everything the frontend needs on startup, in one response.</summary>
    public sealed record InventoryResponse(
        IReadOnlyList<InventoryItemDto> Items,
        IReadOnlyList<EquippedCustomizationDto> Equipped,
        bool IsPro,
        string Version);
}
