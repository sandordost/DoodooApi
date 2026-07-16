using Doodoo.SharedKernel.Enums;

namespace Doodoo.Modules.Todos.Contracts
{
    // Todos-owned result shape for complete/undo. Decoupled from the Currency ledger DTOs.
    public class TodoActionResult
    {
        public TransactionResponseCode ResponseCode { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
