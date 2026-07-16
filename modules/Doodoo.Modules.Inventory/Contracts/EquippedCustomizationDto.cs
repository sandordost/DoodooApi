using Doodoo.Modules.Inventory.Enums;

namespace Doodoo.Modules.Inventory.Contracts
{
    /// <summary>The payload of a currently equipped customization, one per occupied slot.</summary>
    public sealed record EquippedCustomizationDto(
        ItemSlot Slot,
        int EntryId,
        int DefinitionId,
        string Key,
        ContentType ContentType,
        string? Content);
}
