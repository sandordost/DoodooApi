using Doodoo.SharedKernel.Enums;

namespace DoodooApi.Models.Responses.Rewards
{
    // Rewards-owned result shape. Decoupled from the Currency module's ledger DTOs.
    public class RewardTransactionResult
    {
        public TransactionResponseCode ResponseCode { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
