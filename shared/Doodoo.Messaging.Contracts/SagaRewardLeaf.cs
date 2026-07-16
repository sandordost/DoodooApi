using Doodoo.SharedKernel.Enums;

namespace Doodoo.Messaging.Contracts
{
    public record SagaRewardLeaf(Guid ItemId, ItemDifficulty Difficulty);
}
