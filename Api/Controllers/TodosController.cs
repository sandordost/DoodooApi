using Doodoo.Modules.Todos.Contracts;
using Doodoo.Modules.Todos.Entities;
using Doodoo.Modules.Todos.Services;
using DoodooApi.Models.Enums;
using DoodooApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static DoodooApi.Helpers.DateHelpers;

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

            return Ok(items.ToArray());
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
            TodoItem? updatedItem;
            try
            {
                updatedItem = await todoItemService.UpdateItemAsync(id, userId, request);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

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
            TodoItem newItem;
            try
            {
                newItem = await todoItemService.CreateItemAsync(userId, request);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return CreatedAtAction(nameof(GetTodoItems), new { id = newItem.Id }, newItem);
        }

        [HttpPost("{id}/Complete")]
        public async Task<ActionResult<TodoActionResult>> CompleteTodoItem(Guid id)
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
        public async Task<ActionResult<TodoActionResult>> UndoCompleteTodoItem(Guid id)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var transactionResponse = await todoItemService.UndoCompletionAsync(id, userId);

            if (transactionResponse.ResponseCode != TransactionResponseCode.Reverted)
            {
                return BadRequest(transactionResponse);
            }

            return Ok(transactionResponse);
        }

        [HttpPut("Order")]
        public async Task<ActionResult> ReorderItems(ReorderTodoItemsRequest request)
        {
            var userId = userService.GetCurrentUserIdOrThrow();

            var success = await todoItemService.ReorderItemsAsync(userId, request);
            if (!success)
            {
                return BadRequest(
                    new { Message = "Failed to reorder items. Please ensure all ids exist and belong to the same ordering scope." }
                );
            }

            return NoContent();
        }

        [HttpPost("DailyCheck")]
        public async Task<ActionResult<DailyCheckResponse>> DailyCheck()
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var user = await userService.GetCurrentUserAsync();

            if (user == null)
            {
                return Unauthorized(false);
            }

            var today = DateTime.UtcNow.Date;

            var needsDailyReset = user.LastDailyReset == null
                || user.LastDailyReset.Value.Date < today;

            // Returns true when current week number is higher then lastWeeklyReset week number, or when lastWeeklyReset is null
            var needsWeeklyReset = user.LastWeeklyReset == null
                || (user.LastWeeklyReset.Value.Date < today && GetWeekOfYear(user.LastWeeklyReset.Value) < GetWeekOfYear(today));

            var response = new DailyCheckResponse
            {
                DailyHasReset = needsDailyReset,
                WeeklyHasReset = needsWeeklyReset
            };

            if (needsDailyReset)
            {
                await todoItemService.ResetDailyItemsAsync(userId);

                user.LastDailyReset = today;
                await userService.UpdateUserAsync(user);
            }

            if (needsWeeklyReset)
            {
                await todoItemService.ResetWeeklyItemsAsync(userId);
                user.LastWeeklyReset = today;
                await userService.UpdateUserAsync(user);
            }

            return Ok(response);
        }
    }
}
