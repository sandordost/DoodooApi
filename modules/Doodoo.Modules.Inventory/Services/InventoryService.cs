using Doodoo.Messaging.Contracts;
using Doodoo.Modules.Inventory.Contracts;
using Doodoo.Modules.Inventory.Entities;
using Doodoo.Modules.Inventory.Enums;
using Doodoo.SharedKernel.Enums;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Doodoo.Modules.Inventory.Services
{
    public sealed class InventoryService(InventoryDbContext db, IMessageBus bus)
    {
        // ---- Reads -------------------------------------------------------------------

        public async Task<InventoryResponse> GetInventoryAsync(Guid userId)
        {
            var entries = await db.InventoryEntries
                .Include(e => e.Definition)
                .Where(e => e.OwnerId == userId)
                .ToListAsync();

            return BuildResponse(entries);
        }

        // ---- Customizations ----------------------------------------------------------

        public async Task<InventoryOperationResult<InventoryResponse>> EquipAsync(Guid userId, int entryId)
        {
            var entries = await db.InventoryEntries
                .Include(e => e.Definition)
                .Where(e => e.OwnerId == userId)
                .ToListAsync();

            var entry = entries.FirstOrDefault(e => e.Id == entryId);
            if (entry?.Definition == null)
                return InventoryOperationResult<InventoryResponse>.Fail(InventoryOpCode.EntryNotFound);

            if (entry.Definition.Kind != ItemKind.Customization)
                return InventoryOperationResult<InventoryResponse>.Fail(InventoryOpCode.NotACustomization);

            var slot = entry.Definition.Slot;
            var now = DateTime.UtcNow;

            // Max one equipped per slot: unequip the current occupant(s) of this slot.
            foreach (var other in entries.Where(e =>
                         e.IsEquipped && e.Definition?.Slot == slot && e.Id != entry.Id))
            {
                other.IsEquipped = false;
                other.UpdatedAtUtc = now;
            }

            if (!entry.IsEquipped)
            {
                entry.IsEquipped = true;
                entry.UpdatedAtUtc = now;
            }

            await db.SaveChangesAsync();
            return InventoryOperationResult<InventoryResponse>.Ok(BuildResponse(entries));
        }

        public async Task<InventoryOperationResult<InventoryResponse>> UnequipAsync(Guid userId, int entryId)
        {
            var entries = await db.InventoryEntries
                .Include(e => e.Definition)
                .Where(e => e.OwnerId == userId)
                .ToListAsync();

            var entry = entries.FirstOrDefault(e => e.Id == entryId);
            if (entry?.Definition == null)
                return InventoryOperationResult<InventoryResponse>.Fail(InventoryOpCode.EntryNotFound);

            if (entry.Definition.Kind != ItemKind.Customization)
                return InventoryOperationResult<InventoryResponse>.Fail(InventoryOpCode.NotACustomization);

            if (entry.IsEquipped)
            {
                entry.IsEquipped = false;
                entry.UpdatedAtUtc = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            return InventoryOperationResult<InventoryResponse>.Ok(BuildResponse(entries));
        }

        // ---- Consumables -------------------------------------------------------------

        public async Task<InventoryOperationResult<UseItemResponse>> UseAsync(Guid userId, int entryId)
        {
            var entry = await db.InventoryEntries
                .Include(e => e.Definition)
                .FirstOrDefaultAsync(e => e.OwnerId == userId && e.Id == entryId);

            if (entry?.Definition == null)
                return InventoryOperationResult<UseItemResponse>.Fail(InventoryOpCode.EntryNotFound);

            var def = entry.Definition;
            if (def.Kind != ItemKind.Consumable)
                return InventoryOperationResult<UseItemResponse>.Fail(InventoryOpCode.NotAConsumable);

            if (entry.Quantity <= 0)
                return InventoryOperationResult<UseItemResponse>.Fail(InventoryOpCode.OutOfStock);

            decimal? newGold = null;
            int? newSapphires = null;

            // Apply the effect via the owning module BEFORE decrementing, so a failed grant
            // leaves the consumable intact (grant-first, then persist state).
            var currencyType = def.Effect switch
            {
                ConsumableEffect.GrantGold => (CurrencyType?)CurrencyType.Gold,
                ConsumableEffect.GrantSapphires => CurrencyType.Sapphire,
                _ => null
            };

            if (currencyType is not null && def.EffectAmount != 0)
            {
                var result = await bus.InvokeAsync<GrantInventoryCurrencyResult>(
                    new GrantInventoryCurrency(userId,
                        [new CurrencyAmount(currencyType.Value, def.EffectAmount)]));

                if (result.ResponseCode != TransactionResponseCode.Created)
                    return InventoryOperationResult<UseItemResponse>.Fail(InventoryOpCode.CurrencyGrantFailed);

                newGold = result.NewGold;
                newSapphires = result.NewSapphires;
            }

            entry.Quantity -= 1;
            entry.LastUsedAtUtc = DateTime.UtcNow;
            entry.UpdatedAtUtc = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return InventoryOperationResult<UseItemResponse>.Ok(new UseItemResponse(
                entry.Id, entry.Quantity, def.Effect, def.EffectAmount, newGold, newSapphires));
        }

        // ---- Granting ----------------------------------------------------------------

        /// <summary>Grant every active default definition to a (new) user. Idempotent.</summary>
        public async Task GrantDefaultsAsync(Guid userId)
        {
            var defaults = await db.ItemDefinitions
                .Where(d => d.IsDefault && d.IsActive)
                .ToListAsync();

            foreach (var def in defaults)
                await GrantInternalAsync(userId, def, quantity: 1, autoEquip: def.Kind == ItemKind.Customization);

            await db.SaveChangesAsync();
        }

        /// <summary>Grant one definition (by key) to a user. Idempotent per (user, definition).</summary>
        public async Task<InventoryOpCode> GrantItemAsync(Guid userId, string definitionKey, int quantity)
        {
            var def = await db.ItemDefinitions.FirstOrDefaultAsync(d => d.Key == definitionKey);
            if (def == null) return InventoryOpCode.DefinitionNotFound;

            await GrantInternalAsync(userId, def, quantity, autoEquip: false);
            await db.SaveChangesAsync();
            return InventoryOpCode.Success;
        }

        /// <summary>
        /// Grant a definition to every user who currently owns any inventory. Every registered
        /// user receives the defaults on registration, so this covers all existing users.
        /// </summary>
        public async Task<InventoryOpCode> GrantItemToAllAsync(string definitionKey, int quantity)
        {
            var def = await db.ItemDefinitions.FirstOrDefaultAsync(d => d.Key == definitionKey);
            if (def == null) return InventoryOpCode.DefinitionNotFound;

            var userIds = await db.InventoryEntries
                .Select(e => e.OwnerId)
                .Distinct()
                .ToListAsync();

            foreach (var userId in userIds)
                await GrantInternalAsync(userId, def, quantity, autoEquip: false);

            await db.SaveChangesAsync();
            return InventoryOpCode.Success;
        }

        private async Task GrantInternalAsync(Guid userId, ItemDefinition def, int quantity, bool autoEquip)
        {
            var existing = await db.InventoryEntries
                .FirstOrDefaultAsync(e => e.OwnerId == userId && e.DefinitionId == def.Id);

            if (existing != null)
            {
                if (def.Stackable)
                {
                    existing.Quantity += quantity;
                    existing.UpdatedAtUtc = DateTime.UtcNow;
                }
                return; // non-stackable already owned: idempotent no-op
            }

            db.InventoryEntries.Add(new InventoryEntry
            {
                OwnerId = userId,
                DefinitionId = def.Id,
                Quantity = def.Stackable ? quantity : 1,
                IsEquipped = autoEquip,
                AcquiredAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            });
        }

        // ---- Admin: definitions ------------------------------------------------------

        public Task<List<ItemDefinition>> ListDefinitionsAsync() =>
            db.ItemDefinitions.OrderBy(d => d.Id).ToListAsync();

        public async Task<InventoryOperationResult<ItemDefinition>> CreateDefinitionAsync(CreateItemDefinitionRequest req)
        {
            if (await db.ItemDefinitions.AnyAsync(d => d.Key == req.Key))
                return InventoryOperationResult<ItemDefinition>.Fail(InventoryOpCode.DuplicateKey);

            var def = new ItemDefinition
            {
                Key = req.Key,
                Name = req.Name,
                Description = req.Description,
                Kind = req.Kind,
                Slot = req.Slot,
                Category = req.Category,
                ContentType = req.ContentType,
                Content = req.Content,
                Effect = req.Effect,
                EffectAmount = req.EffectAmount,
                Stackable = req.Stackable,
                UnlockAtLevel = req.UnlockAtLevel,
                IsDefault = req.IsDefault,
                IsActive = req.IsActive
            };

            db.ItemDefinitions.Add(def);
            await db.SaveChangesAsync();
            return InventoryOperationResult<ItemDefinition>.Ok(def);
        }

        public async Task<InventoryOperationResult<ItemDefinition>> UpdateDefinitionAsync(int id, UpdateItemDefinitionRequest req)
        {
            var def = await db.ItemDefinitions.FirstOrDefaultAsync(d => d.Id == id);
            if (def == null)
                return InventoryOperationResult<ItemDefinition>.Fail(InventoryOpCode.DefinitionNotFound);

            def.Name = req.Name;
            def.Description = req.Description;
            def.Slot = req.Slot;
            def.Category = req.Category;
            def.ContentType = req.ContentType;
            def.Content = req.Content;
            def.Effect = req.Effect;
            def.EffectAmount = req.EffectAmount;
            def.Stackable = req.Stackable;
            def.UnlockAtLevel = req.UnlockAtLevel;
            def.IsDefault = req.IsDefault;
            def.IsActive = req.IsActive;

            await db.SaveChangesAsync();
            return InventoryOperationResult<ItemDefinition>.Ok(def);
        }

        public async Task<InventoryOpCode> SetActiveAsync(int id, bool isActive)
        {
            var def = await db.ItemDefinitions.FirstOrDefaultAsync(d => d.Id == id);
            if (def == null) return InventoryOpCode.DefinitionNotFound;

            def.IsActive = isActive;
            await db.SaveChangesAsync();
            return InventoryOpCode.Success;
        }

        // ---- Mapping -----------------------------------------------------------------

        private static InventoryResponse BuildResponse(List<InventoryEntry> entries)
        {
            var now = DateTime.UtcNow;

            var items = entries
                .Where(e => e.Definition != null)
                .Select(e => new InventoryItemDto(
                    e.Id, e.DefinitionId, e.Definition!.Key, e.Definition.Name, e.Definition.Description,
                    e.Definition.Kind, e.Definition.Slot, e.Definition.Category,
                    e.Definition.ContentType, e.Definition.Content,
                    e.Definition.Effect, e.Definition.EffectAmount,
                    e.Quantity, e.IsEquipped, e.ExpiresAtUtc))
                .ToList();

            var equipped = entries
                .Where(e => e.IsEquipped && e.Definition is { Kind: ItemKind.Customization })
                .Select(e => new EquippedCustomizationDto(
                    e.Definition!.Slot, e.Id, e.DefinitionId, e.Definition.Key,
                    e.Definition.ContentType, e.Definition.Content))
                .ToList();

            var isPro = entries.Any(e =>
                e.Definition is { Kind: ItemKind.Membership } &&
                (e.ExpiresAtUtc == null || e.ExpiresAtUtc > now));

            var version = entries.Count == 0
                ? "0"
                : entries.Max(e => e.UpdatedAtUtc).Ticks.ToString();

            return new InventoryResponse(items, equipped, isPro, version);
        }
    }
}
