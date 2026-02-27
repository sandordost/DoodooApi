using DoodooApi.Models;
using DoodooApi.Models.CurrencyAccounts;
using DoodooApi.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class CurrencyAccountService(AppDbContext context)
    {
        public async Task<BalanceResponse?> GetBalance(Guid userId)
        {
            var account = await context.CurrencyAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(ca => ca.OwnerId == userId);

            if (account == null)
            {
                return null;
            }

            return new BalanceResponse
            {
                Gold = account.Gold,
                Sapphires = account.Sapphires
            };
        }

        public async Task<List<Transaction>> GetTransactions(Guid userId)
        {
            var accountId = await context.CurrencyAccounts
                .Where(ca => ca.OwnerId == userId)
                .Select(ca => (Guid?)ca.Id)
                .FirstOrDefaultAsync();

            if (accountId == null)
            {
                return [];
            }

            return await context.Transactions
                .Include(t => t.TransactionRecords)
                .Where(t => t.CurrencyAccountId == accountId.Value)
                .OrderByDescending(t => t.CreatedTimestamp)
                .ToListAsync();
        }
    }
}