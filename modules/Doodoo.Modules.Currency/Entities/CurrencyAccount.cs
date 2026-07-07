using DoodooApi.Models.Main.Transactions;

namespace DoodooApi.Models.Main.CurrencyAccounts
{
    public class CurrencyAccount
    {
        public Guid Id { get; set; }

        // Logical reference to a Users-module AppUser. No DB FK / navigation across modules.
        public Guid OwnerId { get; set; }

        public decimal Gold { get; set; } = 0;
        public int Sapphires { get; set; } = 0;
        public List<Transaction> Transactions { get; set; } = [];
    }
}
