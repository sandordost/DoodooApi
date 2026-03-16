using DoodooApi.Models.Main.CurrencyAccounts;
using DoodooApi.Models.Main.Rewards;
using DoodooApi.Models.Main.TodoItems;
using Microsoft.AspNetCore.Identity;

namespace DoodooApi.Models.Main.Users
{
    public class AppUser : IdentityUser<Guid>
    {
        public List<TodoItem> TodoItems { get; set; } = [];
        public CurrencyAccount? CurrencyAccount { get; set; }
        public List<Reward> Rewards { get; set; } = [];
        public List<RewardClaim> RewardClaims { get; set; } = [];
        public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    }
}
