using Doodoo.SharedKernel.Enums;

namespace Doodoo.Messaging.Contracts
{
    public record GrantInventoryCurrencyResult(TransactionResponseCode ResponseCode, Guid? TransactionId, decimal NewGold, int NewSapphires);
}
