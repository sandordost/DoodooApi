using Doodoo.SharedKernel.Enums;
using Doodoo.SharedKernel.Enums.Flags;

namespace Doodoo.Modules.Todos.Contracts
{
    public class CreateTodoItemRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public ItemDifficulty ItemDifficulty { get; set; } = ItemDifficulty.Easy;
        public ItemCategory ItemCategory { get; set; }
        public ActiveDays ActiveDays { get; set; }

        // Saga support: mark this item as a saga (container), and/or nest it under an existing saga.
        // A child inherits its parent saga's ItemCategory.
        public bool IsSaga { get; set; } = false;
        public Guid? ParentId { get; set; }
    }

}
