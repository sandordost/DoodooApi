using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Responses.Todos
{
    public class DailyCheckResponse
    {
        public bool DailyHasReset { get; set; }
        public bool WeeklyHasReset { get; set; }
    }

    // Todos-owned result shape for complete/undo. Decoupled from the Currency ledger DTOs.
    public class TodoActionResult
    {
        public TransactionResponseCode ResponseCode { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
