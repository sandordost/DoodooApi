using DoodooApi.Models.CurrencyAccounts;
using DoodooApi.Models.Rewards;
using DoodooApi.Models.TodoItems;
using Microsoft.AspNetCore.Identity;

namespace DoodooApi.Models.Users
{
    public class AppUser : IdentityUser<Guid>
    {
        public List<TodoItem> TodoItems { get; set; } = [];
        public CurrencyAccount? CurrencyAccount { get; set; }
        public List<Reward> Rewards { get; set; } = [];
        public List<RewardClaim> RewardClaims { get; set; } = [];
    }
}
