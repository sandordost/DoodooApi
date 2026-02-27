using DoodooApi.Models;
using DoodooApi.Models.CurrencyAccounts;
using DoodooApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoodooApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CurrencyAccountController(
        CurrencyAccountService currencyAccountService,
        UserService userService) : ControllerBase
    {
        [HttpGet("balance")]
        public async Task<ActionResult<BalanceResponse>> GetBalance()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var balance = await currencyAccountService.GetBalance(userId);

            if (balance == null)
            {
                return NotFound("Currency account not found.");
            }

            return Ok(balance);
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var transactions = await currencyAccountService.GetTransactions(userId);

            return Ok(transactions);
        }
    }
}