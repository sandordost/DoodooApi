using DoodooApi.Models.Requests.Rewards;
using DoodooApi.Models.Responses.Rewards;
using DoodooApi.Models.Responses.Transactions;
using DoodooApi.Models.Rewards;
using DoodooApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoodooApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RewardsController(RewardService rewardService, UserService userService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RewardResponse>>> GetRewards()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var rewards = await rewardService.GetRewards(userId);

            var response = rewards.Select(reward =>
            {
                return new RewardResponse
                {
                    Name = reward.Name,
                    Description = reward.Description,
                    Icon = reward.Icon,
                    Id = reward.Id,
                    RewardCosts = [.. reward.RewardCosts.Select(rewardCost =>
                    {
                        return new RewardCostResponse
                        {
                            Amount = rewardCost.Amount,
                            CurrencyType = rewardCost.CurrencyType,
                            Id = rewardCost.Id
                        };
                    })]
                };
            }).ToList();

            return Ok(response);
        }

        [HttpDelete("{rewardId:int}")]
        public async Task<ActionResult> DeleteReward(int rewardId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var success = await rewardService.DeleteReward(rewardId, userId);
            if (!success)
            {
                return NotFound("Reward not found or you do not have permission to delete it.");
            }
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<RewardResponse>> CreateReward(CreateRewardRequest request)
        {
            var userId = userService.GetCurrentUserIdOrThrow();

            // Forceer ownership op basis van ingelogde user
            request.OwnerId = userId;

            var newReward = await rewardService.CreateReward(request);

            if (newReward == null)
            {
                return BadRequest("Failed to create reward.");
            }

            var response = new RewardResponse()
            {
                Id = newReward.Id,
                Name = newReward.Name,
                Description = newReward.Description,
                Icon = newReward.Icon,
                RewardCosts = [.. newReward.RewardCosts.Select(rewardCost =>
                    new RewardCostResponse()
                    {
                        Id = rewardCost.Id,
                        CurrencyType = rewardCost.CurrencyType,
                        Amount = rewardCost.Amount
                    }
                )]
            };

            return CreatedAtAction(nameof(GetRewards), new { id = response.Id }, response);
        }

        [HttpPatch("{rewardId:int}")]
        public async Task<ActionResult<RewardResponse>> UpdateReward(int rewardId, UpdateRewardRequest request)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var updatedReward = await rewardService.UpdateReward(request, rewardId, userId);
            if (updatedReward == null)
            {
                return NotFound("Reward not found or you do not have permission to update it.");
            }
            var response = new RewardResponse()
            {
                Id = updatedReward.Id,
                Name = updatedReward.Name,
                Description = updatedReward.Description,
                Icon = updatedReward.Icon,
                RewardCosts = [.. updatedReward.RewardCosts.Select(rewardCost =>
                    new RewardCostResponse()
                    {
                        Id = rewardCost.Id,
                        CurrencyType = rewardCost.CurrencyType,
                        Amount = rewardCost.Amount
                    }
                )]
            };
            return Ok(response);
        }

        [HttpGet("{rewardId:int}/claims")]
        public async Task<ActionResult<IEnumerable<RewardClaimResponse>>> GetRewardClaims(int rewardId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var claims = await rewardService.GetRewardClaimsAsync(rewardId);

            var response = claims.Select(claim =>
            {
                return new RewardClaimResponse
                {
                    Id = claim.Id,
                    ClaimedAt = claim.ClaimedAt,
                    RewardId = claim.RewardId,
                    TransactionId = claim.TransactionId,
                };
            }).ToList();

            return Ok(response);
        }

        [HttpPost("{rewardId:int}/claims")]
        public async Task<ActionResult<ClaimRewardResponse>> ClaimReward(int rewardId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var result = await rewardService.ClaimReward(rewardId, userId);

            return result;
        }

        [HttpPost("claims/{rewardClaimId:int}/undo")]
        public async Task<ActionResult<TransactionProcessResponse>> UndoClaim(int rewardClaimId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var result = await rewardService.UndoRewardClaim(rewardClaimId, userId);

            return result;
        }
    }
}