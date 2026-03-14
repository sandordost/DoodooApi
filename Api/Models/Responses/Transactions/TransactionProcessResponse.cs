using DoodooApi.Models.Enums;

namespace DoodooApi.Models.Responses.Transactions
{
    public class TransactionProcessResponse
    {
        public TransactionResponseCode ResponseCode { get; set; }
        public Guid? TransactionId { get; set; }
        public TransactionResponse? Transaction { get; set; }
    }
}
