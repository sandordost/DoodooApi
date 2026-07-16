namespace Doodoo.Messaging.Contracts
{
    // Rewards -> Currency (InvokeAsync): refund a previously claimed reward.
    public record RefundRewardClaim(Guid TransactionId);
}
