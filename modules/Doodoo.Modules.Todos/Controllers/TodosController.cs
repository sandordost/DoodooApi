using Doodoo.Modules.Todos.Contracts;
using Doodoo.Modules.Todos.Entities;
using Doodoo.Modules.Todos.Services;
using Doodoo.SharedKernel.Abstractions;
using Doodoo.SharedKernel.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Doodoo.Modules.Todos.Helpers.DateHelpers;

namespace Doodoo.Modules.Todos.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TodosController(
        TodoItemService todoItemService,
        ICurrentUser currentUser,
        IUserResetStore userResetStore) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetTodoItems()
        {
            var userId = currentUser.GetCurrentUserIdOrThrow();
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
            var userId = currentUser.GetCurrentUserIdOrThrow();
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
            var userId = currentUser.GetCurrentUserIdOrThrow();
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
            var userId = currentUser.GetCurrentUserIdOrThrow();
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
            var userId = currentUser.GetCurrentUserIdOrThrow();
            var response = await todoItemService.CompleteItemAsync(id, userId);

            if (response.ResponseCode == TransactionResponseCode.Completed)
                return Ok(response);

            return BadRequest(response);
        }

        [HttpPost("{id}/Reset")]
        public async Task<ActionResult> ResetTodoItem(Guid id)
        {
            var userId = currentUser.GetCurrentUserIdOrThrow();
            var response = await todoItemService.ResetItemAsync(id, userId);

            if (response == true)
                return Ok();

            return BadRequest("Could not reset currenct item");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteTodoItem(Guid id)
        {
            var userId = currentUser.GetCurrentUserIdOrThrow();

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
            var userId = currentUser.GetCurrentUserIdOrThrow();
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
            var userId = currentUser.GetCurrentUserIdOrThrow();

            var success = await todoItemService.ReorderItemsAsync(userId, request);
            if (!success)
            {
                return BadRequest(
                    new { Message = "Failed to reorder items. Please ensure all ids exist and belong to the same ordering scope." }
                );
            }

            return NoContent();
        }

        [Obsolete("Use PUT /api/Todos/Order instead.")]
        [HttpPut("{id}/Order/{order}")]
        public async Task<ActionResult> UpdateOrder(Guid id, int order)
        {
            var userId = currentUser.GetCurrentUserIdOrThrow();

#pragma warning disable CS0618
            var success = await todoItemService.SetItemOrder(id, userId, order);
#pragma warning restore CS0618
            if (!success)
            {
                return BadRequest(
                    new { Message = "Failed to update item order. Please ensure the item exists and belongs to you." }
                );
            }

            return NoContent();
        }

        [HttpPost("DailyCheck")]
        public async Task<ActionResult<DailyCheckResponse>> DailyCheck()
        {
            var userId = currentUser.GetCurrentUserIdOrThrow();
            var state = await userResetStore.GetAsync(userId);

            if (state == null)
            {
                return Unauthorized(false);
            }

            var today = DateTime.UtcNow.Date;

            var needsDailyReset = state.LastDailyReset == null
                || state.LastDailyReset.Value.Date < today;

            // Returns true when current week number is higher then lastWeeklyReset week number, or when lastWeeklyReset is null
            var needsWeeklyReset = state.LastWeeklyReset == null
                || (state.LastWeeklyReset.Value.Date < today && GetWeekOfYear(state.LastWeeklyReset.Value) < GetWeekOfYear(today));

            var response = new DailyCheckResponse
            {
                DailyHasReset = needsDailyReset,
                WeeklyHasReset = needsWeeklyReset
            };

            if (needsDailyReset)
            {
                await todoItemService.ResetDailyItemsAsync(userId);
            }

            if (needsWeeklyReset)
            {
                await todoItemService.ResetWeeklyItemsAsync(userId);
            }

            if (needsDailyReset || needsWeeklyReset)
            {
                await userResetStore.SetAsync(
                    userId,
                    needsDailyReset ? today : state.LastDailyReset,
                    needsWeeklyReset ? today : state.LastWeeklyReset);
            }

            return Ok(response);
        }
    }
}
