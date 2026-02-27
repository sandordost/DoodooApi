using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Rewards
{
    public class RewardCost
    {
        public int Id { get; set; }
        public int RewardId { get; set; }
        public Reward? Reward { get; set; }
        public required CurrencyType CurrencyType { get; set; }
        public decimal Amount { get; set; }
    }
}
