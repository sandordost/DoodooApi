using Doodoo.Modules.Currency;
using DoodooApi.Models.Main.CurrencyAccounts;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class CurrencyAccountService(CurrencyDbContext context)
    {
        public async Task<CurrencyAccount> GetCurrencyAccountAsync(Guid userId)
        {
            var account = await context.CurrencyAccounts
                .FirstOrDefaultAsync(ca => ca.OwnerId == userId);

            return account ?? throw new NullReferenceException("User does not have a currency account!");
        }

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
    }
}
