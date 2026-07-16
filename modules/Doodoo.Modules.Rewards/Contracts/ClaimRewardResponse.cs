namespace DoodooApi.Models.Responses.Rewards
{
    public class ClaimRewardResponse
    {
        public int ClaimId { get; set; }
        public required RewardTransactionResult TransactionProcessResponse { get; set; }
    }
}
