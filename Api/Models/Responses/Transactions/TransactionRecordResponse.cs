using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Responses.Transactions
{
    public class TransactionRecordResponse
    {
        public required CurrencyType CurrencyType { get; set; }
        public decimal Value { get; set; }
    }
}
