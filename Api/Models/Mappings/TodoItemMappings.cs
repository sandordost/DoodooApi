using DoodooApi.Models.Main.TodoItems;
using DoodooApi.Models.Requests.TodoItems;

namespace DoodooApi.Models.Mappings
{
    public static class TodoItemMappings
    {
        public static TodoItem ToTodoItem(this CreateTodoItemRequest createTodoItemRequest)
        {
            return new TodoItem
            {
                Title = createTodoItemRequest.Title,
                Description = createTodoItemRequest.Description,
                ItemDifficulty = createTodoItemRequest.ItemDifficulty,
                ItemCategory = createTodoItemRequest.ItemCategory
            };
        }
    }
}
