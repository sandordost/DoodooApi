namespace Doodoo.Messaging.Contracts
{
    // Rewards -> Currency (InvokeAsync): debit currency for a reward claim.
    public record DebitForRewardClaim(Guid UserId, int ClaimId, IReadOnlyList<CurrencyAmount> Costs);
}
