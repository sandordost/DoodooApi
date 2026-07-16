using Doodoo.SharedKernel.Enums;

namespace Doodoo.Messaging.Contracts
{
    public record RewardClaimDebitResult(TransactionResponseCode ResponseCode, Guid? TransactionId);
}
