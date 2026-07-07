using DoodooApi.Models.Enums;

namespace Doodoo.Messaging.Contracts
{
    // Users -> Currency (event, via outbox)
    public record UserRegistered(Guid UserId);

    // Todos -> Currency (InvokeAsync): grant reward for completing a todo item
    public record GrantItemCompletionReward(Guid UserId, Guid ItemId, ItemDifficulty Difficulty, DateTime CompletedAtUtc);
    public record ItemCompletionRewardResult(TransactionResponseCode ResponseCode, Guid? TransactionId, decimal Gold, int Sapphires);

    // Todos -> Currency (InvokeAsync): compensation / undo of a completion grant
    public record RevertItemCompletionReward(Guid UserId, Guid ItemId);
    public record RevertResult(TransactionResponseCode ResponseCode);

    // Rewards -> Currency (InvokeAsync): debit currency for a reward claim
    public record CurrencyAmount(CurrencyType CurrencyType, decimal Value);
    public record DebitForRewardClaim(Guid UserId, int ClaimId, IReadOnlyList<CurrencyAmount> Costs);
    public record RewardClaimDebitResult(TransactionResponseCode ResponseCode, Guid? TransactionId);

    // Rewards -> Currency (InvokeAsync): refund a previously claimed reward
    public record RefundRewardClaim(Guid TransactionId);
    public record RefundResult(TransactionResponseCode ResponseCode);
}
