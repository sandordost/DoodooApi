using Doodoo.SharedKernel.Enums;
using DoodooApi.Models.Main.CurrencyAccounts;

namespace DoodooApi.Models.Main.Transactions
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

}
