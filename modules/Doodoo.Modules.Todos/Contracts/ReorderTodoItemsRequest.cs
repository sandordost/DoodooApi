using Doodoo.SharedKernel.Enums;

namespace Doodoo.Modules.Todos.Contracts
{
    public class ReorderTodoItemsRequest
    {
        public Guid? ParentId { get; set; }
        public ItemCategory? ItemCategory { get; set; }
        public required List<Guid> OrderedIds { get; set; }
    }
}
