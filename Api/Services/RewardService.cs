using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.Rewards;
using DoodooApi.Models.Mappings;
using DoodooApi.Models.Requests.Rewards;
using DoodooApi.Models.Requests.Transactions;
using DoodooApi.Models.Responses.Rewards;
using DoodooApi.Models.Responses.Transactions;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class RewardService(AppDbContext context, TransactionService transactionService)
    {
        public async Task<List<Reward>> GetRewards(Guid userId)
        {
            var rewards = await context.Users.Where(u => u.Id == userId)
                .Include(u => u.Rewards)
                .ThenInclude(r => r.RewardCosts)
                .Select(u => u.Rewards)
                .FirstOrDefaultAsync();

            rewards = rewards?.Where(r => !r.IsDeleted).ToList();

            return rewards ?? throw new NullReferenceException("User's reward collection is not initialized");
        }

        public async Task<Reward> CreateReward(CreateRewardRequest rewardRequest)
        {
            var newReward = rewardRequest.ToReward();

            await context.Rewards.AddAsync(newReward);
            await context.SaveChangesAsync();

            return newReward;
        }
        public async Task<ClaimRewardResponse> ClaimReward(int rewardId, Guid userId)
        {
            var user = await context.Users
                .Include(u => u.Rewards)
                .ThenInclude(r => r.RewardCosts)
                .Include(u => u.CurrencyAccount)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NullReferenceException("User not found");

            var reward = user.Rewards.FirstOrDefault(r => r.Id == rewardId && !r.IsDeleted);

            var response = new ClaimRewardResponse
            {
                TransactionProcessResponse = new()
                {
                    ResponseCode = TransactionResponseCode.ItemNotFound
                }
            };

            if (reward == null)
            {
                response.TransactionProcessResponse.ResponseCode = TransactionResponseCode.ItemNotFound;
                return response;
            }

            if (user.CurrencyAccount == null)
            {
                response.TransactionProcessResponse.ResponseCode = TransactionResponseCode.CurrencyAccountNotFound;
                return response;
            }

            var requiredGold = reward.RewardCosts
                .Where(rc => rc.CurrencyType == CurrencyType.Gold)
                .Sum(rc => rc.Amount);

            var requiredSapphires = reward.RewardCosts
                .Where(rc => rc.CurrencyType == CurrencyType.Sapphire)
                .Sum(rc => (int)rc.Amount);

            if (user.CurrencyAccount.Gold < requiredGold || user.CurrencyAccount.Sapphires < requiredSapphires)
            {
                response.TransactionProcessResponse.ResponseCode = TransactionResponseCode.InsufficientFunds;
                return response;
            }

            var claim = new RewardClaim
            {
                RewardId = reward.Id,
                UserId = userId,
            };

            await context.RewardClaims.AddAsync(claim);
            await context.SaveChangesAsync();

            response.ClaimId = claim.Id;

            var transactionRequest = new CreateTransactionRequest
            {
                SourceType = TransactionSourceType.RewardClaim,
                SourceIdInt = claim.Id,
                CurrencyAccountId = user.CurrencyAccount.Id,
                TransactionRecords = [.. reward.RewardCosts.Select(rc =>
            new TransactionRecordRequest
            {
                CurrencyType = rc.CurrencyType,
                Value = -rc.Amount,
            })]
            };

            var transactionResponse = await transactionService.MakeTransactionAsync(transactionRequest);

            if (transactionResponse.ResponseCode != TransactionResponseCode.Created)
            {
                context.RewardClaims.Remove(claim);
                await context.SaveChangesAsync();

                response.TransactionProcessResponse = transactionResponse;
                return response;
            }

            if (transactionResponse.TransactionId == null)
                throw new NullReferenceException("Transaction was not created successfully");

            claim.TransactionId = transactionResponse.TransactionId.Value;
            await context.SaveChangesAsync();

            response.TransactionProcessResponse = transactionResponse;
            return response;
        }

        public async Task<TransactionProcessResponse> UndoRewardClaim(int claimId, Guid userId)
        {
            var user = await context.Users
                .Include(u => u.RewardClaims)
                .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new NullReferenceException("User not found");

            var claim = user.RewardClaims.FirstOrDefault(rc => rc.Id == claimId);

            if (claim == null)
            {
                return new()
                {
                    ResponseCode = TransactionResponseCode.AlreadyReverted
                };
            }

            if (claim.TransactionId == null)
            {
                context.RewardClaims.Remove(claim);
                await context.SaveChangesAsync();
                return new() { ResponseCode = TransactionResponseCode.NoTransactionFound };
            }

            var undoResult = await transactionService.UndoTransactionAsync(claim.TransactionId.Value);

            if (undoResult != TransactionResponseCode.Deleted)
            {
                return new TransactionProcessResponse
                {
                    ResponseCode = undoResult
                };
            }

            context.RewardClaims.Remove(claim);
            await context.SaveChangesAsync();

            return new() { ResponseCode = TransactionResponseCode.Reverted };
        }

        public async Task<IEnumerable<RewardClaim>> GetRewardClaimsAsync(int rewardId)
        {
            return await context.RewardClaims
                .Where(rc => rc.RewardId == rewardId)
                .ToListAsync();
        }

        public async Task<bool> DeleteReward(int rewardId, Guid userId)
        {
            var reward = await context.Rewards
                .Include(r => r.Owner)
                .FirstOrDefaultAsync(r => r.Id == rewardId && !r.IsDeleted);

            if (reward?.Owner == null || reward.Owner.Id != userId)
            {
                return false;
            }

            reward.IsDeleted = true;
            await context.SaveChangesAsync();

            return true;
        }
    }
}