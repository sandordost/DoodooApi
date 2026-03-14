using static DoodooApi.Services.RewardService;

namespace DoodooApi.Models.Responses.Rewards
{
    public class RewardResponse()
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<RewardCostResponse> RewardCosts { get; set; } = [];
    }
}
