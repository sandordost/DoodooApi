using Doodoo.SharedKernel.Enums;

namespace Doodoo.Messaging.Contracts
{
    public record ItemCompletionRewardResult(TransactionResponseCode ResponseCode, Guid? TransactionId, decimal Gold, int Sapphires);
}
