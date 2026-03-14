using DoodooApi.Models.Main.Rewards;
using DoodooApi.Models.Requests.Rewards;
using DoodooApi.Models.Rewards;

namespace DoodooApi.Models.Mappings
{
    public static class RewardMappings
    {
        public static Reward ToReward(this CreateRewardRequest request)
        {
            return new Reward
            {
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon,
                OwnerId = request.OwnerId,
                RewardCosts = [.. request.RewardCosts.Select(rc => new RewardCost
                {
                    CurrencyType = rc.CurrencyType,
                    Amount = rc.Amount
                })]
            };
        }
    }
}
