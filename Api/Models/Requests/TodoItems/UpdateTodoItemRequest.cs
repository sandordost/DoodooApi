using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Requests.TodoItems
{
    public class UpdateTodoItemRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ItemDifficulty? ItemDifficulty { get; set; }
        public ItemCategory? ItemCategory { get; set; }
    }
}
