using DoodooApi.Models.Enums;

namespace Doodoo.Messaging.Contracts
{
    // Currency -> Rewards (InvokeAsync): compute the difficulty-based reward amounts.
    // Rewards owns the DifficultyRewardRule table and the deterministic RNG.
    public record CalculateDifficultyReward(Guid UserId, Guid ItemId, ItemDifficulty Difficulty, DateTime DateUtc);
    public record DifficultyRewardAmounts(decimal Gold, int Sapphires);
}
