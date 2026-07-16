using Doodoo.SharedKernel.Enums;

namespace Doodoo.Messaging.Contracts
{
    public record CurrencyAmount(CurrencyType CurrencyType, decimal Value);
}
