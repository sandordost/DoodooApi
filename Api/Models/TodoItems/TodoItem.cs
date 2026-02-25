using DoodooApi.Models.Enums;
using DoodooApi.Models.Users;

namespace DoodooApi.Models.TodoItems
{
    public class TodoItem
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public ItemDifficulty ItemDifficulty { get; set; }
        public Guid OwnerId { get; set; }
        public AppUser? Owner { get; set; }
        public DateTime? CompletedTimestamp { get; set; }
        public DateTime? DeletedTimestamp { get; set; }
        public ItemCategory ItemCategory { get; set; }
    }
}
