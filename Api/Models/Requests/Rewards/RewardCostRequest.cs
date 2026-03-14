using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Requests.Rewards
{
    public class RewardCostRequest
    {
        public required CurrencyType CurrencyType { get; set; }
        public decimal Amount { get; set; }
    }
}
