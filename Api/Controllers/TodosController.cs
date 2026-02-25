using DoodooApi.Models.TodoItems;
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

        [HttpGet("/{id}")]
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

        [HttpPost]
        public async Task<ActionResult<TodoItem>> CreateTodoItem(TodoItemService.CreateTodoItemRequest request)
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
        public async Task<ActionResult> CompleteTodoItem(Guid id)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var success = await todoItemService.CompleteItemAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("/{id}")]
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
        public async Task<ActionResult> UndoCompleteTodoItem(Guid id)
        {
            var userId = userService.GetCurrentUserIdOrThrow();
            var success = await todoItemService.UndoCompletionAsync(id, userId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
