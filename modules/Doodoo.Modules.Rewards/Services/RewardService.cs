using Doodoo.Messaging.Contracts;
using Doodoo.Modules.Rewards;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.Rewards;
using DoodooApi.Models.Mappings;
using DoodooApi.Models.Requests.Rewards;
using DoodooApi.Models.Responses.Rewards;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace DoodooApi.Services
{
    public class RewardService(RewardsDbContext context, IMessageBus bus)
    {
        public async Task<List<Reward>> GetRewards(Guid userId)
        {
            return await context.Rewards
                .Include(r => r.RewardCosts)
                .Where(r => r.OwnerId == userId && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<Reward> CreateReward(CreateRewardRequest rewardRequest)
        {
            var newReward = rewardRequest.ToReward();

            await context.Rewards.AddAsync(newReward);
            await context.SaveChangesAsync();

            return newReward;
        }

        public async Task<Reward> UpdateReward(UpdateRewardRequest rewardRequest, int rewardId, Guid userId)
        {
            var reward = await context.Rewards
                .Include(r => r.RewardCosts)
                .FirstOrDefaultAsync(r => r.Id == rewardId && !r.IsDeleted);

            if (reward == null || reward.OwnerId != userId)
            {
                throw new NullReferenceException("Reward not found or user is not the owner");
            }

            reward.Name = rewardRequest.Name;
            reward.Description = rewardRequest.Description;
            reward.Icon = rewardRequest.Icon;

            context.RewardCosts.RemoveRange(reward.RewardCosts);
            reward.RewardCosts = [.. rewardRequest.RewardCosts.Select(c => new RewardCost
            {
                CurrencyType = c.CurrencyType,
                Amount = c.Amount,
                RewardId = reward.Id
            })];

            await context.SaveChangesAsync();
            return reward;
        }

        public async Task<ClaimRewardResponse> ClaimReward(int rewardId, Guid userId)
        {
            var reward = await context.Rewards
                .Include(r => r.RewardCosts)
                .FirstOrDefaultAsync(r => r.Id == rewardId && r.OwnerId == userId && !r.IsDeleted);

            var response = new ClaimRewardResponse
            {
                TransactionProcessResponse = new()
                {
                    ResponseCode = TransactionResponseCode.ItemNotFound
                }
            };

            if (reward == null)
            {
                return response;
            }

            // Record the claim first, then ask Currency to debit. The Currency module owns
            // the funds check + debit atomically and is idempotent on the claim id.
            var claim = new RewardClaim
            {
                RewardId = reward.Id,
                UserId = userId,
            };

            await context.RewardClaims.AddAsync(claim);
            await context.SaveChangesAsync();

            response.ClaimId = claim.Id;

            var costs = reward.RewardCosts
                .Select(rc => new CurrencyAmount(rc.CurrencyType, rc.Amount))
                .ToList();

            var debit = await bus.InvokeAsync<RewardClaimDebitResult>(
                new DebitForRewardClaim(userId, claim.Id, costs));

            if (debit.ResponseCode != TransactionResponseCode.Created)
            {
                // Compensation: the debit failed (e.g. insufficient funds), so drop the claim.
                context.RewardClaims.Remove(claim);
                await context.SaveChangesAsync();

                response.TransactionProcessResponse = new() { ResponseCode = debit.ResponseCode };
                return response;
            }

            claim.TransactionId = debit.TransactionId;
            await context.SaveChangesAsync();

            response.TransactionProcessResponse = new()
            {
                ResponseCode = debit.ResponseCode,
                TransactionId = debit.TransactionId
            };
            return response;
        }

        public async Task<RewardTransactionResult> UndoRewardClaim(int claimId, Guid userId)
        {
            var claim = await context.RewardClaims
                .FirstOrDefaultAsync(rc => rc.Id == claimId && rc.UserId == userId);

            if (claim == null)
            {
                return new() { ResponseCode = TransactionResponseCode.AlreadyReverted };
            }

            if (claim.TransactionId == null)
            {
                context.RewardClaims.Remove(claim);
                await context.SaveChangesAsync();
                return new() { ResponseCode = TransactionResponseCode.NoTransactionFound };
            }

            var refund = await bus.InvokeAsync<RefundResult>(new RefundRewardClaim(claim.TransactionId.Value));

            if (refund.ResponseCode != TransactionResponseCode.Deleted)
            {
                return new() { ResponseCode = refund.ResponseCode };
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
                .FirstOrDefaultAsync(r => r.Id == rewardId && !r.IsDeleted);

            if (reward == null || reward.OwnerId != userId)
            {
                return false;
            }

            reward.IsDeleted = true;
            await context.SaveChangesAsync();

            return true;
        }
    }
}
