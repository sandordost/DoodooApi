using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Responses.Transactions;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class DifficultyRewardService(AppDbContext context)
    {

        public async Task<List<TransactionRecordResponse>> GetRewardsByDifficultyAsync(ItemDifficulty difficulty)
        {
            var difficultyRewardRule = await context.DifficultyRewardRules.FirstOrDefaultAsync(r => r.Difficulty == difficulty)
                ?? throw new NullReferenceException("DifficultyRewardRule not found! Should be fixed asap");

            var goldGained = difficultyRewardRule.GoldAmount;

            bool sapphiresGained = Random.Shared.NextDouble() <= difficultyRewardRule.SapphireChance;
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
    }
}
