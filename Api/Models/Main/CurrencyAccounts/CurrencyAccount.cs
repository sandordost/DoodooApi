using DoodooApi.Models.Main.Transactions;
using DoodooApi.Models.Main.Users;

namespace DoodooApi.Models.Main.CurrencyAccounts
{
    public class CurrencyAccount
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public AppUser? Owner { get; set; }
        public decimal Gold { get; set; } = 0;
        public int Sapphires { get; set; } = 0;
        public List<Transaction> Transactions { get; set; } = [];

    }
}
