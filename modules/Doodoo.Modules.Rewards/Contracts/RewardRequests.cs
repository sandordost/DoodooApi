using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Requests.Rewards
{
    public class CreateRewardRequest
    {
        public Guid OwnerId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<RewardCostRequest> RewardCosts { get; set; } = [];
    }

    public class UpdateRewardRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<RewardCostRequest> RewardCosts { get; set; } = [];
    }

    public class RewardCostRequest
    {
        public required CurrencyType CurrencyType { get; set; }
        public decimal Amount { get; set; }
    }
}
