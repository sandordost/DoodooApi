namespace DoodooApi.Models.Rewards
{
    public class RewardClaimResponse
    {
        public int Id { get; set; }
        public int RewardId { get; set; }
        public Guid? TransactionId { get; set; }
        public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    }
}
