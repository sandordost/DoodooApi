using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Requests.Transactions
{
    public class CreateTransactionRequest
    {
        public required TransactionSourceType SourceType { get; set; }
        public Guid? SourceIdGuid { get; set; }
        public int? SourceIdInt { get; set; }
        public required Guid CurrencyAccountId { get; set; }
        public List<TransactionRecordRequest> TransactionRecords { get; set; } = [];
    }
}
