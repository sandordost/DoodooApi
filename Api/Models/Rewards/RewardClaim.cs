using DoodooApi.Models.Users;

namespace DoodooApi.Models.Rewards
{
    public class RewardClaim
    {
        public int Id { get; set; }
        public int RewardId { get; set; }
        public Reward? Reward { get; set; }
        public Guid TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public Guid UserId { get; set; }
        public AppUser? User { get; set; }
        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    }
}
