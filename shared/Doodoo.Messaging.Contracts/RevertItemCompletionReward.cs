namespace Doodoo.Messaging.Contracts
{
    // Todos -> Currency (InvokeAsync): compensation / undo of a completion grant (also used to
    // claw back a saga aggregate; the transaction is keyed on the item/saga id).
    public record RevertItemCompletionReward(Guid UserId, Guid ItemId);
}
