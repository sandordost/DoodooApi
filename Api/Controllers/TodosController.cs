using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.TodoItems;
using DoodooApi.Models.Requests.TodoItems;
using DoodooApi.Models.Responses.Transactions;
using DoodooApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoodooApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TodosController(TodoItemService todoItemService, UserService userService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var items = await todoItemService.GetItemsAsync(userId);

            if (items == null)
            {
                return NotFound();
            }

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> GetTodoItem(Guid id)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var item = await todoItemService.GetItemAsync(id, userId);

            if (item == null)
            {
                return NotFound();
            }

            return Ok(item);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<TodoItem>> UpdateTodoItem(Guid id, UpdateTodoItemRequest request)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var updatedItem = await todoItemService.UpdateItemAsync(id, userId, request);

            if (updatedItem == null)
            {
                return NotFound();
            }
            return Ok(updatedItem);
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(CreateTodoItemRequest request)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var newItem = await todoItemService.CreateItemAsync(userId, request);

            if (newItem == null)
            {
                return BadRequest("Failed to create item");
            }

            return CreatedAtAction(nameof(GetTodoItems), new { id = newItem.Id }, newItem);
        }

        [HttpPost("{id}/Complete")]
        public async Task<ActionResult<TransactionProcessResponse>> CompleteTodoItem(Guid id)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var response = await todoItemService.CompleteItemAsync(id, userId);

            if (response.ResponseCode == TransactionResponseCode.Completed)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("{id}/Reset")]
        public async Task<ActionResult> ResetTodoItem(Guid id)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var response = await todoItemService.ResetItemAsync(id, userId);

            if (response == true)
                return Ok();

            return BadRequest("Could not reset currenct item");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoItem(Guid id)
        {
            var userId = userService.GetCurrentUserIdOrThrow();

            var success = await todoItemService.DeleteItemAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("{id}/UndoCompletion")]
        public async Task<ActionResult<TransactionProcessResponse>> UndoCompleteTodoItem(Guid id)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var transactionResponse = await todoItemService.UndoCompletionAsync(id, userId);

            if (transactionResponse.ResponseCode != TransactionResponseCode.Reverted)
            {
                return BadRequest(transactionResponse);
            }

            return Ok(transactionResponse);
        }

        [HttpPost("DailyCheck")]
        public async Task<ActionResult<bool>> DailyCheck()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var user = await userService.GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(false);
            }

            var today = DateTime.UtcNow.Date;

            var needsReset = user.LastDailyReset == null
                || user.LastDailyReset.Value.Date < today;

            if (!needsReset)
            {
                return Ok(false);
            }

            await todoItemService.ResetDailyItemsAsync(userId);

            user.LastDailyReset = today;
            await userService.UpdateUserAsync(user);

            return Ok(true);
        }
    }
}
