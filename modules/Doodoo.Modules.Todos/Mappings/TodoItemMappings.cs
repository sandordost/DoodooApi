using Doodoo.Modules.Todos.Contracts;
using Doodoo.Modules.Todos.Entities;

namespace Doodoo.Modules.Todos.Mappings
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
