using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Responses.Transactions
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public required TransactionSourceType SourceType { get; set; }
        public int? SourceIdInt { get; set; }
        public Guid? SourceIdGuid { get; set; }
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
        public required Guid CurrencyAccountId { get; set; }
        public List<TransactionRecordResponse> TransactionRecords { get; set; } = [];
    }

    public class TransactionRecordResponse
    {
        public required CurrencyType CurrencyType { get; set; }
        public decimal Value { get; set; }
    }

    public class TransactionProcessResponse
    {
        public TransactionResponseCode ResponseCode { get; set; }
        public Guid? TransactionId { get; set; }
        public TransactionResponse? Transaction { get; set; }
    }
}
