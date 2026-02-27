using AutoMapper;
using DoodooApi.Models;
using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class TransactionService(AppDbContext context, IMapper mapper)
    {
        public record CreateTransactionRequest
        {
            public required TransactionSourceType SourceType { get; set; }
            public Guid? SourceIdGuid { get; set; }
            public int? SourceIdInt { get; set; }
            public required Guid CurrencyAccountId { get; set; }
            public List<TransactionRecord> TransactionRecords { get; set; } = [];
        }

        public record TransactionResponse
        {
            public TransactionResponseCode ResponseCode { get; set; }
            public Guid? TransactionId { get; set; }
        }

        public async Task<TransactionResponse> MakeTransaction(CreateTransactionRequest transactionRequest)
        {
            var transaction = mapper.Map<Transaction>(transactionRequest);

            var currencyAccountUpdated = await HandleCurrencyAccountChanges(transaction);

            if (!currencyAccountUpdated)
            {
                return new()
                {
                    ResponseCode = TransactionResponseCode.CurrencyAccountNotFound,
                    TransactionId = null
                };
            }

            await context.Transactions.AddAsync(transaction);
            await context.SaveChangesAsync();

            return new()
            {
                ResponseCode = TransactionResponseCode.Created,
                TransactionId = transaction.Id
            };
        }

        public async Task<TransactionResponseCode> UndoTransaction(Guid transactionId)
        {
            var transaction = await context.Transactions
                .Include(t => t.TransactionRecords)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null)
            {
                return TransactionResponseCode.NoTransactionFound;
            }

            var currencyAccountUpdated = await HandleCurrencyAccountChanges(transaction, reverse: true);

            if (!currencyAccountUpdated)
            {
                return TransactionResponseCode.CurrencyAccountNotFound;
            }

            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();

            return TransactionResponseCode.Deleted;
        }

        private async Task<bool> HandleCurrencyAccountChanges(Transaction transaction, bool reverse = false)
        {
            var currencyAccount = await context.CurrencyAccounts
                .FirstOrDefaultAsync(ca => ca.Id == transaction.CurrencyAccountId);

            if (currencyAccount == null)
            {
                return false;
            }

            foreach (var transactionRecord in transaction.TransactionRecords ?? Enumerable.Empty<TransactionRecord>())
            {
                var multiplier = reverse ? -1 : 1;

                switch (transactionRecord.CurrencyType)
                {
                    case CurrencyType.Gold:
                        currencyAccount.Gold += transactionRecord.Value * multiplier;
                        break;

                    case CurrencyType.Sapphire:
                        currencyAccount.Sapphires += (int)Math.Round(
                            transactionRecord.Value,
                            MidpointRounding.AwayFromZero) * multiplier;
                        break;
                }
            }

            return true;
        }
    }
}
