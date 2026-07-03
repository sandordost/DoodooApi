using Doodoo.Modules.Rewards;
using DoodooApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public sealed class DifficultyRewardService(RewardsDbContext context)
    {
        public async Task<(decimal Gold, int Sapphires)> GetAmountsAsync(
            ItemDifficulty difficulty, Guid? userId = null, Guid? itemId = null, DateTime? date = null)
        {
            var rng = new Random();
            if (userId.HasValue && itemId.HasValue && date.HasValue)
            {
                rng = new Random(CreateDeterministicSeed(userId.Value, itemId.Value, date.Value.Date));
            }

            var difficultyRewardRule = await context.DifficultyRewardRules
                .FirstOrDefaultAsync(r => r.Difficulty == difficulty)
                ?? throw new NullReferenceException("DifficultyRewardRule not found! Should be fixed asap");

            var goldGained = difficultyRewardRule.GoldAmount;

            bool sapphiresGained = rng.NextDouble() <= difficultyRewardRule.SapphireChance;
            var sapphiresGainedAmount = sapphiresGained ? difficultyRewardRule.SapphireAmount : 0;

            return (goldGained, sapphiresGainedAmount);
        }

        // IMPORTANT: do not change the seed string format or date normalization —
        // it would silently alter historical sapphire-drop outcomes for the same day.
        private static int CreateDeterministicSeed(Guid userId, Guid itemId, DateTime date)
        {
            var input = $"{userId}_{itemId}_{date:yyyyMMdd}";
            var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));

            return BitConverter.ToInt32(hash, 0);
        }
    }
}
