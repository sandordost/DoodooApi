using Doodoo.SharedKernel.Enums;

namespace DoodooApi.Models.Responses.Rewards
{
    public class RewardResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<RewardCostResponse> RewardCosts { get; set; } = [];
    }

    public class RewardCostResponse
    {
        public int Id { get; set; }
        public CurrencyType CurrencyType { get; set; }
        public decimal Amount { get; set; }
    }

    // Rewards-owned result shape. Decoupled from the Currency module's ledger DTOs.
    public class RewardTransactionResult
    {
        public TransactionResponseCode ResponseCode { get; set; }
        public Guid? TransactionId { get; set; }
    }

    public class ClaimRewardResponse
    {
        public int ClaimId { get; set; }
        public required RewardTransactionResult TransactionProcessResponse { get; set; }
    }
}
