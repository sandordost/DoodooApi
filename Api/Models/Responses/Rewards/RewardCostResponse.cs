using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Responses.Rewards
{
    public class RewardCostResponse
    {
        public int Id { get; set; }
        public CurrencyType CurrencyType { get; set; }
        public decimal Amount { get; set; }
    }
}
