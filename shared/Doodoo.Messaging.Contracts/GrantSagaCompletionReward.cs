namespace Doodoo.Messaging.Contracts
{
    // Todos -> Currency (InvokeAsync): grant the aggregate reward when a root saga completes.
    // Currency computes each leaf's difficulty reward and writes one transaction keyed on SagaId.
    public record GrantSagaCompletionReward(Guid UserId, Guid SagaId, IReadOnlyList<SagaRewardLeaf> Leaves, DateTime CompletedAtUtc);
}
