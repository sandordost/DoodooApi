using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.Transactions;
using DoodooApi.Models.Mappings;
using DoodooApi.Models.Requests.Transactions;
using DoodooApi.Models.Responses.Transactions;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class TransactionService(AppDbContext context)
    {

        public async Task<TransactionProcessResponse> MakeTransactionAsync(CreateTransactionRequest transactionRequest)
        {
            var transaction = transactionRequest.ToTransaction();

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
                TransactionId = transaction.Id,
                Transaction = transaction.ToTransactionResponse()
            };
        }

        public async Task<TransactionResponseCode> UndoTransactionAsync(Guid transactionId)
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

        public async Task<Transaction?> GetTransactionBySourceAsync(TransactionSourceType itemCompletion, Guid id)
        {
            return await context.Transactions
                .Include(t => t.TransactionRecords)
                .FirstOrDefaultAsync(t =>
                    t.SourceType == itemCompletion &&
                    t.SourceIdGuid == id);
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

        public async Task<List<Transaction>> GetTransactionsByUserIdAsync(Guid userId, int limit = 50)
        {
            return await context.Transactions
                .Include(t => t.TransactionRecords)
                .Where(t =>
                    t.CurrencyAccount != null &&
                    t.CurrencyAccount.OwnerId == userId)
                .OrderByDescending(t => t.CreatedTimestamp)
                .Take(limit)
                .ToListAsync();
        }
    }
}
