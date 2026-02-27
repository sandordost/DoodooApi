using DoodooApi.Models.Enums;
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
        public async Task<ActionResult<IEnumerable<Reward>>> GetRewards()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var rewards = await rewardService.GetRewards(userId);

            return Ok(rewards);
        }

        [HttpPost]
        public async Task<ActionResult<Reward>> CreateReward(RewardService.CreateRewardRequest request)
        {
            var userId = userService.GetCurrentUserIdOrThrow();

            // Forceer ownership op basis van ingelogde user
            request.OwnerId = userId;

            var reward = await rewardService.CreateReward(request);

            if (reward == null)
            {
                return BadRequest("Failed to create reward.");
            }

            return CreatedAtAction(nameof(GetRewards), new { id = reward.Id }, reward);
        }

        [HttpPost("{rewardId:int}/claim")]
        public async Task<ActionResult<TransactionResponseCode>> ClaimReward(int rewardId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var result = await rewardService.ClaimReward(rewardId, userId);

            return result;
        }

        [HttpPost("claims/{rewardClaimId:int}/undo")]
        public async Task<ActionResult<TransactionResponseCode>> UndoClaim(int rewardClaimId)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var result = await rewardService.UndoClaim(rewardClaimId, userId);

            return result;
        }
    }
}