using Doodoo.SharedKernel.Abstractions;
using DoodooApi.Models.Main.CurrencyAccounts;
using DoodooApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Doodoo.Modules.Currency.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CurrencyAccountController(
        CurrencyAccountService currencyAccountService,
        ICurrentUser currentUser) : ControllerBase
    {
        [HttpGet("balance")]
        public async Task<ActionResult<BalanceResponse>> GetBalance()
        {
            var userId = currentUser.GetCurrentUserIdOrThrow();
            var balance = await currencyAccountService.GetBalance(userId);

            if (balance == null)
            {
                return NotFound("Currency account not found.");
            }

            return Ok(balance);
        }
    }
}
