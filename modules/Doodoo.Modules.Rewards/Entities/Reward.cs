namespace DoodooApi.Models.Main.Rewards
{
    public class Reward
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        // Logical reference to a Users-module AppUser. No DB FK / navigation across modules.
        public required Guid OwnerId { get; set; }

        public string? Description { get; set; }
        public string? Icon { get; set; }
        public List<RewardCost> RewardCosts { get; set; } = [];
        public bool IsDeleted { get; set; } = false;
        public List<RewardClaim> Claims { get; set; } = [];
    }
}
