using Doodoo.SharedKernel.Enums;

namespace Doodoo.Messaging.Contracts
{
    // Todos -> Currency (InvokeAsync): grant reward for completing a todo item.
    public record GrantItemCompletionReward(Guid UserId, Guid ItemId, ItemDifficulty Difficulty, DateTime CompletedAtUtc);
}
