using DoodooApi.Models.Mappings;
using DoodooApi.Models.Responses.Transactions;
using DoodooApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoodooApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TransactionsController(UserService userService, TransactionService transactionService) : Controller
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionResponse>>> GetTransactions()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var transactions = await transactionService.GetTransactionsByUserIdAsync(userId);

            var transactionResponses = transactions.Select(t => t.ToTransactionResponse());

            return Ok(transactionResponses);
        }
    }
}
