using Doodoo.SharedKernel.Enums;

namespace DoodooApi.Models.Main.Transactions
{
    public class TransactionRecord
    {
        public int Id { get; set; }
        public Guid TransactionId { get; set; }
        public Transaction? Transaction { get; set; }
        public required CurrencyType CurrencyType { get; set; }
        public decimal Value { get; set; }
    }
}
