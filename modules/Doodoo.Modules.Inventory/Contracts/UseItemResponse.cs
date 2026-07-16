using Doodoo.Modules.Inventory.Enums;

namespace Doodoo.Modules.Inventory.Contracts
{
    public sealed record UseItemResponse(
        Guid UseId,
        int EntryId,
        int RemainingQuantity,
        ConsumableEffect Effect,
        int EffectAmount,
        decimal? NewGoldBalance,
        int? NewSapphireBalance);
}
