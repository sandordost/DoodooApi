using Doodoo.Messaging.Contracts;
using Doodoo.Modules.Inventory.Services;

namespace Doodoo.Modules.Inventory
{
    public static class InventoryHandler
    {
        // Users -> Inventory (event). Grant the default items to a newly registered user.
        // Runs in the same UserRegistered chain as the Currency handler; idempotent.
        public static async Task Handle(UserRegistered message, InventoryService inventoryService)
        {
            await inventoryService.GrantDefaultsAsync(message.UserId);
        }
    }
}
