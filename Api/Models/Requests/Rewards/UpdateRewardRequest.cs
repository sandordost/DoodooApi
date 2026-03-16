namespace DoodooApi.Models.Requests.Rewards
{
    public class UpdateRewardRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<RewardCostRequest> RewardCosts { get; set; } = [];
    }
}
