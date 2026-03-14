using DoodooApi.Models.Responses.Transactions;

namespace DoodooApi.Models.Responses.Rewards
{
    public class ClaimRewardResponse
    {
        public int ClaimId { get; set; }
        public required TransactionProcessResponse TransactionProcessResponse { get; set; }
    }
}
