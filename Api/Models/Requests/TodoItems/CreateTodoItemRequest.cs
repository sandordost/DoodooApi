using DoodooApi.Models.Enums;
using DoodooApi.Models.Enums.Flags;

namespace DoodooApi.Models.Requests.TodoItems
{
    public class CreateTodoItemRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public ItemDifficulty ItemDifficulty { get; set; } = ItemDifficulty.Easy;
        public ItemCategory ItemCategory { get; set; }
        public ActiveDays ActiveDays { get; set; }
    }
}
