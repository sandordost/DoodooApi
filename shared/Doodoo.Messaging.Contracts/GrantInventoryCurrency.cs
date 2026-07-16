namespace Doodoo.Messaging.Contracts
{
    // Inventory -> Currency (InvokeAsync): credit currency when a consumable is used
    // (e.g. a "bag of coins" grants gold). Append-only credit (SourceType = InventoryUse).
    public record GrantInventoryCurrency(Guid UserId, IReadOnlyList<CurrencyAmount> Amounts);
}
