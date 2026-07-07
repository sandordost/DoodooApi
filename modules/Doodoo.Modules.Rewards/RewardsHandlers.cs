using Doodoo.Messaging.Contracts;
using DoodooApi.Services;

namespace Doodoo.Modules.Rewards
{
    public static class RewardsHandler
    {
        // Currency -> Rewards (request/response). Rewards owns the difficulty rule + deterministic RNG.
        public static async Task<DifficultyRewardAmounts> Handle(
            CalculateDifficultyReward message,
            DifficultyRewardService difficultyRewardService)
        {
            var (gold, sapphires) = await difficultyRewardService.GetAmountsAsync(
                message.Difficulty, message.UserId, message.ItemId, message.DateUtc);

            return new DifficultyRewardAmounts(gold, sapphires);
        }
    }
}
