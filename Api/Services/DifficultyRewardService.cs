using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Responses.Transactions;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class DifficultyRewardService(AppDbContext context)
    {

        public async Task<List<TransactionRecordResponse>> GetRewardsByDifficultyAsync(ItemDifficulty difficulty, Guid? userId = null, Guid? itemId = null, DateTime? date = null)
        {
            var rng = new Random();
            if (userId.HasValue && itemId.HasValue && date.HasValue)
            {
                rng = new Random(CreateDeterministicSeed(userId.Value, itemId.Value, date.Value.Date));
            }

            var difficultyRewardRule = await context.DifficultyRewardRules.FirstOrDefaultAsync(r => r.Difficulty == difficulty)
                ?? throw new NullReferenceException("DifficultyRewardRule not found! Should be fixed asap");

            var goldGained = difficultyRewardRule.GoldAmount;

            bool sapphiresGained = rng.NextDouble() <= difficultyRewardRule.SapphireChance;
            var sapphiresGainedAmount = sapphiresGained ? difficultyRewardRule.SapphireAmount : 0;

            var rewards = new List<TransactionRecordResponse>();

            if (goldGained > 0)
            {
                rewards.Add(new TransactionRecordResponse
                {
                    CurrencyType = CurrencyType.Gold,
                    Value = goldGained
                });
            }

            if (sapphiresGainedAmount > 0)
            {
                rewards.Add(new TransactionRecordResponse
                {
                    CurrencyType = CurrencyType.Sapphire,
                    Value = sapphiresGainedAmount
                });
            }

            return rewards;
        }

        private static int CreateDeterministicSeed(Guid userId, Guid itemId, DateTime date)
        {
            var input = $"{userId}_{itemId}_{date:yyyyMMdd}";
            var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(input));

            return BitConverter.ToInt32(hash, 0);
        }
    }
}
