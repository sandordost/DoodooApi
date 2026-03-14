namespace DoodooApi.Models.Enums
{
    public enum TransactionResponseCode
    {
        Created = 0,
        Completed = 1,
        Deleted = 2,
        ItemNotFound = 3,
        NoTransactionFound = 4,
        AlreadyCompleted = 5,
        AlreadyReverted = 6,
        CurrencyAccountNotFound = 7,
        Reverted = 8,
        InsufficientFunds = 9,
    }
}
