namespace DoodooApi.Models.Main.Rewards
{
    public class RewardClaim
    {
        public int Id { get; set; }

        public int RewardId { get; set; }
        public Reward? Reward { get; set; }

        // Logical reference to a Currency-module Transaction. No DB FK / navigation across modules.
        public Guid? TransactionId { get; set; }

        // Logical reference to a Users-module AppUser.
        public Guid UserId { get; set; }

        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    }
}
