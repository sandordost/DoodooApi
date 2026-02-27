using DoodooApi.Models.CurrencyAccounts;
using DoodooApi.Models.Enums;

namespace DoodooApi.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public required TransactionSourceType SourceType { get; set; }
        public int? SourceIdInt { get; set; }
        public Guid? SourceIdGuid { get; set; }
        public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
        public CurrencyAccount? CurrencyAccount { get; set; }
        public required Guid CurrencyAccountId { get; set; }
        public List<TransactionRecord> TransactionRecords { get; set; } = [];
    }

    public class TransactionRecord
    {
        public int Id { get; set; }
        public Guid TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public required CurrencyType CurrencyType { get; set; }
        public decimal Value { get; set; }
    }
}
