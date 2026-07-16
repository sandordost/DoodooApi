using Doodoo.SharedKernel.Enums;
using Doodoo.SharedKernel.Enums.Flags;

namespace Doodoo.Modules.Todos.Contracts
{
    public class UpdateTodoItemRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public ItemDifficulty? ItemDifficulty { get; set; }
        public ItemCategory? ItemCategory { get; set; }
        public ActiveDays? ActiveDays { get; set; }
        public bool? IsSaga { get; set; }
        public Guid? ParentId { get; set; }
    }
}
