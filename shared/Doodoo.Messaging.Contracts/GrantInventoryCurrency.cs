namespace Doodoo.Messaging.Contracts
{
    // Inventory -> Currency (InvokeAsync): credit currency when a consumable is used
    // (e.g. a "bag of coins" grants gold). UseId is persisted as SourceIdGuid for idempotency.
    public record GrantInventoryCurrency(Guid UserId, Guid UseId, IReadOnlyList<CurrencyAmount> Amounts);
}
