using System.Text.Json.Serialization;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Enums.Flags;

namespace Doodoo.Modules.Todos.Entities
{
    public class TodoItem
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public ItemDifficulty ItemDifficulty { get; set; }

        // Logical reference to a Users-module AppUser. No DB FK / navigation across modules.
        public Guid OwnerId { get; set; }

        public DateTime? CompletedTimestamp { get; set; }
        public DateTime? LastCompletedTimestamp { get; set; }
        public DateTime? PreviousCompletedTimestamp { get; set; }
        public DateTime? DeletedTimestamp { get; set; }
        public ItemCategory ItemCategory { get; set; }
        public int? DailyStreak { get; set; }
        public int? WeeklyStreak { get; set; }
        public DateTime? LastWeeklyCheck { get; set; }
        public DateTime? LastResetDate { get; set; }
        public ActiveDays ActiveDays { get; set; } = ActiveDays.EveryDay;
        public int Order { get; set; } = 0;

        // Saga support: a saga is a container item that cannot be completed directly; it
        // auto-completes when all its children are complete. Children (regular todos or nested
        // sagas) reference their parent. A whole saga tree shares one ItemCategory.
        public bool IsSaga { get; set; } = false;
        public Guid? ParentId { get; set; }

        // Navigation only (for EF/tree traversal). Not serialized: the API returns a flat list and
        // the frontend rebuilds the tree from ParentId. Serializing these would create cycles.
        [JsonIgnore] public TodoItem? Parent { get; set; }
        [JsonIgnore] public List<TodoItem> Children { get; set; } = [];
    }
}
