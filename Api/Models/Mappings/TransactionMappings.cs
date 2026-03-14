using DoodooApi.Models.Main.Transactions;
using DoodooApi.Models.Requests.Transactions;
using DoodooApi.Models.Responses.Transactions;

namespace DoodooApi.Models.Mappings
{
    public static class TransactionMappings
    {
        public static Transaction ToTransaction(this CreateTransactionRequest request)
        {
            return new Transaction
            {
                CurrencyAccountId = request.CurrencyAccountId,
                SourceType = request.SourceType,
                SourceIdGuid = request.SourceIdGuid,
                SourceIdInt = request.SourceIdInt,
                TransactionRecords = [.. request.TransactionRecords.Select(r => r.ToTransactionRecord())]
            };
        }

        public static TransactionRecord ToTransactionRecord(this TransactionRecordRequest request)
        {
            return new TransactionRecord
            {
                CurrencyType = request.CurrencyType,
                Value = request.Value
            };
        }

        public static TransactionResponse ToTransactionResponse(this Transaction transaction)
        {
            return new TransactionResponse
            {
                Id = transaction.Id,
                CurrencyAccountId = transaction.CurrencyAccountId,
                SourceType = transaction.SourceType,
                SourceIdGuid = transaction.SourceIdGuid,
                SourceIdInt = transaction.SourceIdInt,
                CreatedTimestamp = transaction.CreatedTimestamp,
                TransactionRecords = [.. transaction.TransactionRecords.Select(tr => tr.ToTransactionRecordResponse())]
            };
        }

        public static TransactionRecordResponse ToTransactionRecordResponse(this TransactionRecord transactionRecord)
        {
            return new TransactionRecordResponse
            {
                CurrencyType = transactionRecord.CurrencyType,
                Value = transactionRecord.Value
            };
        }
    }
}
