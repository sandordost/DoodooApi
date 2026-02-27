using DoodooApi.Models.Users;

namespace DoodooApi.Models.CurrencyAccounts
{
    public class CurrencyAccount
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public AppUser? Owner { get; set; }
        public decimal Gold { get; set; }
        public int Sapphires { get; set; }
        public List<Transaction> Transactions { get; set; } = [];

    }
}
