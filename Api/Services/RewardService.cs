using AutoMapper;
using DoodooApi.Models;
using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Rewards;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class RewardService(AppDbContext context, IMapper mapper, TransactionService transactionService)
    {
        public async Task<List<Reward>> GetRewards(Guid userId)
        {
            var rewards = await context.Users.Where(u => u.Id == userId).Select(u => u.Rewards).FirstOrDefaultAsync();

            rewards = rewards?.Where(r => !r.IsDeleted).ToList();

            return rewards ?? throw new NullReferenceException("User's reward collection is not initialized");
        }

        public record CreateRewardRequest()
        {
            public Guid OwnerId { get; set; }
            public required string Name { get; set; }
            public string? Description { get; set; }
            public string? Icon { get; set; }
            public List<RewardCost> RewardCosts { get; set; } = [];
        }

        public async Task<Reward?> CreateReward(CreateRewardRequest rewardRequest)
        {
            var newReward = mapper.Map<Reward>(rewardRequest);

            await context.Rewards.AddAsync(newReward);
            await context.SaveChangesAsync();

            return newReward;
        }

        public async Task<TransactionResponseCode> ClaimReward(int rewardId, Guid userId)
        {
            var user = await context.Users
                .Include(u => u.Rewards)
                .Include(u => u.CurrencyAccount)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new NullReferenceException("User not found");

            var reward = user.Rewards.FirstOrDefault(r => r.Id == rewardId && !r.IsDeleted);

            if (reward == null) return TransactionResponseCode.ItemNotFound;
            if (user.CurrencyAccount == null) return TransactionResponseCode.CurrencyAccountNotFound;

            var transactionRequest = new TransactionService.CreateTransactionRequest
            {
                SourceType = TransactionSourceType.RewardClaim,
                SourceIdInt = reward.Id,
                CurrencyAccountId = user.CurrencyAccount.Id,
                TransactionRecords = [.. reward.RewardCosts.Select(rc =>
                new TransactionRecord
                {
                    CurrencyType = rc.CurrencyType,
                    Value = -rc.Amount,
                })]
            };

            var transactionResponse = await transactionService.MakeTransaction(transactionRequest);

            if (transactionResponse.ResponseCode != TransactionResponseCode.Created)
            {
                return transactionResponse.ResponseCode;
            }

            if (transactionResponse.TransactionId == null) throw new NullReferenceException("Transaction was not created successfully");

            var claim = new RewardClaim
            {
                RewardId = reward.Id,
                TransactionId = transactionResponse.TransactionId.Value,
            };

            await context.RewardClaims.AddAsync(claim);
            await context.SaveChangesAsync();

            return transactionResponse.ResponseCode;
        }

        public async Task<TransactionResponseCode> UndoClaim(int claimId, Guid userId)
        {
            var user = await context.Users
                .Include(u => u.RewardClaims)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new NullReferenceException("User not found");

            var claim = user.RewardClaims.FirstOrDefault(rc => rc.Id == claimId);

            if (claim == null)
            {
                return TransactionResponseCode.ItemNotFound;
            }

            var undoResult = await transactionService.UndoTransaction(claim.TransactionId);

            if (undoResult != TransactionResponseCode.Deleted)
            {
                return undoResult;
            }

            context.RewardClaims.Remove(claim);
            await context.SaveChangesAsync();

            return TransactionResponseCode.Deleted;
        }
    }
}