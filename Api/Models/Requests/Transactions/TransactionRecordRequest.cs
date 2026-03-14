using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Requests.Transactions
{
    public class TransactionRecordRequest
    {
        public required CurrencyType CurrencyType { get; set; }
        public decimal Value { get; set; }
    }
}
