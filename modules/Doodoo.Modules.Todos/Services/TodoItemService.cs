using Doodoo.Messaging.Contracts;
using Doodoo.Modules.Todos.Contracts;
using Doodoo.Modules.Todos.Entities;
using Doodoo.Modules.Todos.Mappings;
using DoodooApi.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using static Doodoo.Modules.Todos.Helpers.ActiveDaysHelper;

namespace Doodoo.Modules.Todos.Services
{
    public class TodoItemService(TodosDbContext context, IMessageBus bus)
    {
        public async Task<List<TodoItem>> GetItemsAsync(Guid userId)
        {
            return await context.TodoItems
                .Where(i => i.OwnerId == userId && i.DeletedTimestamp == null)
                .OrderBy(i => i.ItemCategory)
                .ThenBy(i => i.ParentId)
                .ThenBy(i => i.Order)
                .ThenBy(i => i.Id)
                .ToListAsync();
        }

        public async Task<TodoItem?> GetItemAsync(Guid itemId, Guid userId)
        {
            return await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);
        }

        public async Task<TodoActionResult> CompleteItemAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null)
                return new() { ResponseCode = TransactionResponseCode.ItemNotFound };

            // A saga is a container: it auto-completes via its children and cannot be checked directly.
            if (item.IsSaga)
                return new() { ResponseCode = TransactionResponseCode.SagaNotDirectlyCompletable };

            if (item.CompletedTimestamp != null)
                return new() { ResponseCode = TransactionResponseCode.AlreadyCompleted };

            var completedAt = DateTime.UtcNow;
            var today = completedAt.Date;

            if (!IsActiveOn(item.ActiveDays, today.DayOfWeek) && item.ItemCategory == ItemCategory.Daily)
                return new() { ResponseCode = TransactionResponseCode.AlreadyCompleted };

            if (!await IsNextCompletableLeafAsync(userId, item))
                return new() { ResponseCode = TransactionResponseCode.TodoOutOfOrder };

            var isRoot = item.ParentId == null;
            Guid? txId = null;
            Guid? grantedRootId = null;

            // Reward rule: only a completed item WITHOUT a parent (root) pays out. A root leaf pays
            // its own difficulty; children pay nothing (their difficulty bubbles up to the root saga).
            if (isRoot)
            {
                var grant = await bus.InvokeAsync<ItemCompletionRewardResult>(
                    new GrantItemCompletionReward(userId, item.Id, item.ItemDifficulty, completedAt));

                if (grant.ResponseCode != TransactionResponseCode.Created)
                    return new() { ResponseCode = grant.ResponseCode };

                txId = grant.TransactionId;
                grantedRootId = item.Id;
            }

            ApplyStreakOnComplete(item, today);
            item.PreviousCompletedTimestamp = item.LastCompletedTimestamp;
            item.CompletedTimestamp = completedAt;
            item.LastCompletedTimestamp = completedAt;

            // A child completion may auto-complete ancestor sagas; the root saga pays the aggregate.
            if (!isRoot)
            {
                var (failure, grantedRoot) = await PropagateCompletionUpAsync(userId, item, completedAt);
                if (failure is { } failureCode)
                    return new() { ResponseCode = failureCode }; // not saved: no partial completion, no payout
                grantedRootId = grantedRoot;
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch
            {
                // Compensate any reward granted above (root leaf or root saga aggregate) if the
                // Todos save fails. Reverts are keyed on the root id and are idempotent.
                if (grantedRootId is { } rid)
                    await bus.InvokeAsync<RevertResult>(new RevertItemCompletionReward(userId, rid));
                throw;
            }

            return new()
            {
                TransactionId = txId,
                ResponseCode = TransactionResponseCode.Completed
            };
        }

        public async Task<TodoActionResult> UndoCompletionAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return new() { ResponseCode = TransactionResponseCode.ItemNotFound };

            // A saga can only be undone through its children.
            if (item.IsSaga)
                return new() { ResponseCode = TransactionResponseCode.SagaNotDirectlyCompletable };

            if (item.CompletedTimestamp == null)
                return new() { ResponseCode = TransactionResponseCode.AlreadyReverted };

            var isRoot = item.ParentId == null;

            if (isRoot)
            {
                // Root leaf: revert its own reward.
                var revert = await bus.InvokeAsync<RevertResult>(
                    new RevertItemCompletionReward(userId, item.Id));

                if (revert.ResponseCode != TransactionResponseCode.Deleted)
                    return new() { ResponseCode = revert.ResponseCode };
            }

            item.DailyStreak -= 1;
            item.CompletedTimestamp = null;
            item.LastCompletedTimestamp = item.PreviousCompletedTimestamp;

            // Reverting a child un-completes ancestor sagas; the root saga's aggregate is clawed back.
            if (!isRoot)
                await PropagateRevertUpAsync(userId, item);

            await context.SaveChangesAsync();

            return new() { ResponseCode = TransactionResponseCode.Reverted };
        }

        public async Task<bool> DeleteItemAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return false;

            var now = DateTime.UtcNow;
            var affectedParentId = item.ParentId;
            var affectedCategory = item.ItemCategory;

            // Soft-delete the whole subtree (a saga takes its children with it).
            var byParent = await LoadChildLookupAsync(userId);
            foreach (var node in Subtree(item, byParent))
            {
                node.DeletedTimestamp = now;
            }
            await context.SaveChangesAsync();
            await NormalizeScopeAsync(userId, affectedParentId, affectedCategory);

            // Removing items can change the parent saga's completeness.
            if (item.ParentId is { } parentId)
                await ReevaluateSagaAsync(userId, parentId, clawback: true);

            return true;
        }

        public async Task<TodoItem> CreateItemAsync(Guid userId, CreateTodoItemRequest request)
        {
            var newItem = request.ToTodoItem();

            newItem.Id = Guid.NewGuid();
            newItem.OwnerId = userId;
            newItem.CompletedTimestamp = null;
            newItem.DeletedTimestamp = null;
            newItem.ActiveDays = request.ActiveDays;
            newItem.IsSaga = request.IsSaga;

            if (request.ParentId is { } parentId)
            {
                var parent = await GetSagaParentOrThrowAsync(userId, parentId);
                newItem.ParentId = parent.Id;
                newItem.ItemCategory = parent.ItemCategory; // children inherit the saga's cadence
            }

            newItem.Order = await GetNextOrderAsync(userId, newItem.ParentId, newItem.ItemCategory);

            context.TodoItems.Add(newItem);
            await context.SaveChangesAsync();

            // A new (incomplete) child can make a previously-complete saga incomplete again.
            if (newItem.ParentId is { } pid)
                await ReevaluateSagaAsync(userId, pid, clawback: true);

            return newItem;
        }

        public async Task<TodoItem?> UpdateItemAsync(Guid id, Guid userId, UpdateTodoItemRequest request)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == id && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null)
                return null;

            var originalParentId = item.ParentId;
            var originalCategory = item.ItemCategory;
            var scopeChanged = false;

            // Transform into a saga: only allowed for a not-yet-completed item.
            if (request.IsSaga is { } isSaga && isSaga != item.IsSaga)
            {
                if (isSaga && item.CompletedTimestamp != null)
                    throw new InvalidOperationException("A completed item cannot be turned into a saga.");
                item.IsSaga = isSaga;
            }

            // Re-parent: only when a concrete parent is supplied (PATCH can't distinguish "omitted"
            // from "null", so moving to root is not supported via update). Validate target saga,
            // prevent cycles, inherit the saga's category.
            if (request.ParentId is { } newParentId && newParentId != item.ParentId)
            {
                if (await WouldCreateCycleAsync(item.Id, newParentId, userId))
                    throw new InvalidOperationException("Moving the item there would create a cycle.");

                var parent = await GetSagaParentOrThrowAsync(userId, newParentId);
                item.ParentId = parent.Id;
                item.ItemCategory = parent.ItemCategory;
                item.Order = await GetNextOrderAsync(userId, item.ParentId, item.ItemCategory);
                scopeChanged = true;
            }

            item.Title = request.Title ?? item.Title;
            item.Description = request.Description ?? item.Description;
            item.ItemDifficulty = request.ItemDifficulty ?? item.ItemDifficulty;
            item.ActiveDays = request.ActiveDays ?? item.ActiveDays;

            // Category can only be set on a root item (children inherit their tree's cadence).
            // Changing a root saga's category cascades to the whole subtree.
            if (request.ItemCategory is { } category && category != item.ItemCategory)
            {
                if (item.ParentId != null)
                    throw new InvalidOperationException("A child's category is inherited from its saga and cannot be set directly.");

                item.ItemCategory = category;
                item.Order = await GetNextOrderAsync(userId, null, item.ItemCategory, excludeItemId: item.Id);
                scopeChanged = true;
                if (item.IsSaga)
                    await CascadeCategoryAsync(userId, item);
            }

            await context.SaveChangesAsync();

            if (scopeChanged)
            {
                await NormalizeScopeAsync(userId, originalParentId, originalCategory);
                await NormalizeScopeAsync(userId, item.ParentId, item.ItemCategory);
            }

            if (item.ParentId is { } pid)
                await ReevaluateSagaAsync(userId, pid, clawback: true);

            return item;
        }

        public async Task<bool> ResetItemAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return false;

            item.CompletedTimestamp = null;
            await context.SaveChangesAsync();
            return true;
        }

        public async Task ResetDailyItemsAsync(Guid userId)
        {
            var today = DateTime.UtcNow.Date;

            var dailyItems = await context.TodoItems
                .Where(i => i.OwnerId == userId
                    && i.ItemCategory == ItemCategory.Daily
                    && i.DeletedTimestamp == null)
                .ToListAsync();

            foreach (var item in dailyItems)
            {
                ProcessWeeklyReset(item, today);

                if (!IsActiveOn(item.ActiveDays, today.DayOfWeek)) continue;
                if (item.LastResetDate?.Date == today) continue;

                ProcessDailyReset(item, today);

                // Reset (period rollover) makes items available again; it never claws back reward.
                // A whole saga tree shares one category, so sagas + children clear in the same pass.
                item.CompletedTimestamp = null;
                item.LastResetDate = today;
            }

            await context.SaveChangesAsync();
        }

        public async Task ResetWeeklyItemsAsync(Guid userId)
        {
            var today = DateTime.UtcNow.Date;
            var currentWeekStart = StartOfWeek(today);

            var weeklyItems = await context.TodoItems
                .Where(i => i.OwnerId == userId
                    && i.ItemCategory == ItemCategory.Weekly
                    && i.DeletedTimestamp == null)
                .ToListAsync();

            foreach (var item in weeklyItems)
            {
                ProcessWeeklyReset(item, today);

                // Clear a completion that belongs to a previous week so the item can be checked
                // off again this week. A completion made in the current week is preserved.
                // No clawback: a reset is not a revert. LastCompletedTimestamp is kept for streaks.
                if (item.CompletedTimestamp is { } completed && completed.Date < currentWeekStart)
                {
                    item.CompletedTimestamp = null;
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task<bool> ReorderItemsAsync(Guid userId, ReorderTodoItemsRequest request)
        {
            if (request.OrderedIds.Count != request.OrderedIds.Distinct().Count())
                return false;

            Guid? parentId = null;
            ItemCategory category;

            if (request.ParentId is { } requestedParentId)
            {
                var parent = await context.TodoItems
                    .FirstOrDefaultAsync(t => t.Id == requestedParentId
                        && t.OwnerId == userId
                        && t.DeletedTimestamp == null);

                if (parent is not { IsSaga: true })
                    return false;

                parentId = parent.Id;
                category = parent.ItemCategory;
            }
            else
            {
                if (request.ItemCategory is not { } requestedCategory)
                    return false;

                category = requestedCategory;
            }

            var siblings = await GetScopeItemsAsync(userId, parentId, category);
            var siblingIds = siblings.Select(i => i.Id).OrderBy(id => id).ToArray();
            var orderedIds = request.OrderedIds.OrderBy(id => id).ToArray();

            if (!siblingIds.SequenceEqual(orderedIds))
                return false;

            var orderById = request.OrderedIds
                .Select((id, index) => new { id, index })
                .ToDictionary(x => x.id, x => x.index);

            await using var transaction = await context.Database.BeginTransactionAsync();

            foreach (var sibling in siblings)
                sibling.Order = -1 - orderById[sibling.Id];

            await context.SaveChangesAsync();

            foreach (var sibling in siblings)
                sibling.Order = orderById[sibling.Id];

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }

        private async Task<List<TodoItem>> GetScopeItemsAsync(Guid userId, Guid? parentId, ItemCategory category)
        {
            var query = context.TodoItems
                .Where(t => t.OwnerId == userId && t.DeletedTimestamp == null);

            query = parentId is { } pid
                ? query.Where(t => t.ParentId == pid)
                : query.Where(t => t.ParentId == null && t.ItemCategory == category);

            return await query
                .OrderBy(t => t.Order)
                .ThenBy(t => t.Id)
                .ToListAsync();
        }

        private async Task<int> GetNextOrderAsync(
            Guid userId,
            Guid? parentId,
            ItemCategory category,
            Guid? excludeItemId = null)
        {
            var items = await GetScopeItemsAsync(userId, parentId, category);

            if (excludeItemId is { } excludedId)
                items = items.Where(i => i.Id != excludedId).ToList();

            return items.Count;
        }

        private async Task NormalizeScopeAsync(Guid userId, Guid? parentId, ItemCategory category)
        {
            var items = await GetScopeItemsAsync(userId, parentId, category);
            var changedItems = items
                .Select((item, index) => new { item, index })
                .Where(x => x.item.Order != x.index)
                .ToList();

            if (changedItems.Count == 0)
                return;

            await using var transaction = await context.Database.BeginTransactionAsync();

            foreach (var entry in changedItems)
                entry.item.Order = -1 - entry.index;

            await context.SaveChangesAsync();

            foreach (var entry in changedItems)
                entry.item.Order = entry.index;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        private async Task<bool> IsNextCompletableLeafAsync(Guid userId, TodoItem item)
        {
            if (await HasEarlierIncompleteSiblingAsync(userId, item.ParentId, item.ItemCategory, item.Id, item.Order))
                return false;

            var node = await LoadParentAsync(userId, item);

            while (node is not null)
            {
                if (await HasEarlierIncompleteSiblingAsync(userId, node.ParentId, node.ItemCategory, node.Id, node.Order))
                    return false;

                node = await LoadParentAsync(userId, node);
            }

            return true;
        }

        private async Task<bool> HasEarlierIncompleteSiblingAsync(
            Guid userId,
            Guid? parentId,
            ItemCategory category,
            Guid itemId,
            int order)
        {
            var query = context.TodoItems
                .Where(t => t.OwnerId == userId
                    && t.DeletedTimestamp == null
                    && t.Id != itemId
                    && t.Order < order
                    && t.CompletedTimestamp == null);

            query = parentId is { } pid
                ? query.Where(t => t.ParentId == pid)
                : query.Where(t => t.ParentId == null && t.ItemCategory == category);

            return await query.AnyAsync();
        }

        // ---------------- Saga helpers ----------------

        // Walks up from a just-completed child, auto-completing ancestor sagas. When the root saga
        // completes it pays the aggregate. Returns a failure code if the payout was rejected (so the
        // caller aborts without persisting), otherwise the granted root id (for save-failure compensation).
        private async Task<(TransactionResponseCode? Failure, Guid? GrantedRootId)> PropagateCompletionUpAsync(
            Guid userId, TodoItem child, DateTime completedAt)
        {
            var node = await LoadParentAsync(userId, child);
            while (node is { IsSaga: true })
            {
                if (!await AllChildrenCompleteAsync(userId, node.Id))
                    break;

                node.CompletedTimestamp = completedAt;

                if (node.ParentId == null)
                {
                    var leaves = await GatherLeafRewardsAsync(userId, node);
                    var grant = await bus.InvokeAsync<ItemCompletionRewardResult>(
                        new GrantSagaCompletionReward(userId, node.Id, leaves, completedAt));

                    if (grant.ResponseCode != TransactionResponseCode.Created)
                    {
                        node.CompletedTimestamp = null; // don't complete the saga without a payout
                        return (grant.ResponseCode, null);
                    }

                    return (null, node.Id);
                }

                node = await LoadParentAsync(userId, node);
            }

            return (null, null);
        }

        private async Task PropagateRevertUpAsync(Guid userId, TodoItem child)
        {
            var node = await LoadParentAsync(userId, child);
            while (node is { IsSaga: true, CompletedTimestamp: not null })
            {
                node.CompletedTimestamp = null;

                if (node.ParentId == null)
                {
                    await bus.InvokeAsync<RevertResult>(new RevertItemCompletionReward(userId, node.Id));
                }

                node = await LoadParentAsync(userId, node);
            }
        }

        // Re-derive completeness of a saga chain after a structural change (child added/removed).
        private async Task ReevaluateSagaAsync(Guid userId, Guid startParentId, bool clawback)
        {
            var node = await context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == startParentId && t.OwnerId == userId && t.DeletedTimestamp == null);

            var now = DateTime.UtcNow;

            while (node is { IsSaga: true })
            {
                var complete = await AllChildrenCompleteAsync(userId, node.Id);
                var wasComplete = node.CompletedTimestamp != null;

                if (complete == wasComplete)
                    break; // no state change → nothing propagates further up

                if (complete)
                {
                    node.CompletedTimestamp = now;
                    if (node.ParentId == null)
                    {
                        var leaves = await GatherLeafRewardsAsync(userId, node);
                        var grant = await bus.InvokeAsync<ItemCompletionRewardResult>(
                            new GrantSagaCompletionReward(userId, node.Id, leaves, now));

                        if (grant.ResponseCode != TransactionResponseCode.Created)
                        {
                            // Don't persist a completed saga without a ledger entry.
                            node.CompletedTimestamp = null;
                            break;
                        }
                    }
                }
                else
                {
                    node.CompletedTimestamp = null;
                    if (node.ParentId == null && clawback)
                        await bus.InvokeAsync<RevertResult>(new RevertItemCompletionReward(userId, node.Id));
                }

                node = await LoadParentAsync(userId, node);
            }

            await context.SaveChangesAsync();
        }

        private async Task<bool> AllChildrenCompleteAsync(Guid userId, Guid sagaId)
        {
            var children = await context.TodoItems
                .Where(t => t.ParentId == sagaId && t.OwnerId == userId && t.DeletedTimestamp == null)
                .ToListAsync();

            return children.Count > 0 && children.All(c => c.CompletedTimestamp != null);
        }

        private async Task<List<SagaRewardLeaf>> GatherLeafRewardsAsync(Guid userId, TodoItem root)
        {
            var byParent = await LoadChildLookupAsync(userId);
            var leaves = new List<SagaRewardLeaf>();
            var stack = new Stack<TodoItem>();
            foreach (var c in byParent[root.Id]) stack.Push(c);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (node.IsSaga)
                {
                    foreach (var c in byParent[node.Id]) stack.Push(c);
                }
                else
                {
                    leaves.Add(new SagaRewardLeaf(node.Id, node.ItemDifficulty));
                }
            }

            return leaves;
        }

        private async Task<TodoItem?> LoadParentAsync(Guid userId, TodoItem item)
        {
            if (item.ParentId is not { } parentId) return null;
            return await context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == parentId && t.OwnerId == userId && t.DeletedTimestamp == null);
        }

        private async Task<TodoItem> GetSagaParentOrThrowAsync(Guid userId, Guid parentId)
        {
            var parent = await context.TodoItems
                .FirstOrDefaultAsync(t => t.Id == parentId && t.OwnerId == userId && t.DeletedTimestamp == null)
                ?? throw new InvalidOperationException("Parent item not found.");

            if (!parent.IsSaga)
                throw new InvalidOperationException("Items can only be nested under a saga.");

            return parent;
        }

        private async Task<bool> WouldCreateCycleAsync(Guid itemId, Guid newParentId, Guid userId)
        {
            Guid? currentId = newParentId;
            while (currentId is { } cid)
            {
                if (cid == itemId) return true;
                currentId = await context.TodoItems
                    .Where(t => t.Id == cid && t.OwnerId == userId)
                    .Select(t => t.ParentId)
                    .FirstOrDefaultAsync();
            }
            return false;
        }

        private async Task CascadeCategoryAsync(Guid userId, TodoItem saga)
        {
            var byParent = await LoadChildLookupAsync(userId);
            var stack = new Stack<TodoItem>();
            foreach (var c in byParent[saga.Id]) stack.Push(c);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                node.ItemCategory = saga.ItemCategory;
                foreach (var c in byParent[node.Id]) stack.Push(c);
            }
        }

        private async Task<ILookup<Guid, TodoItem>> LoadChildLookupAsync(Guid userId)
        {
            var all = await context.TodoItems
                .Where(t => t.OwnerId == userId && t.DeletedTimestamp == null)
                .ToListAsync();

            return all.Where(t => t.ParentId != null).ToLookup(t => t.ParentId!.Value);
        }

        private static IEnumerable<TodoItem> Subtree(TodoItem root, ILookup<Guid, TodoItem> byParent)
        {
            var stack = new Stack<TodoItem>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                foreach (var c in byParent[node.Id]) stack.Push(c);
            }
        }

        private static void ApplyStreakOnComplete(TodoItem item, DateTime today)
        {
            var previousActiveDate = GetPreviousActiveDate(item.ActiveDays, today);
            var lastCompletedDate = item.LastCompletedTimestamp?.Date;

            item.DailyStreak =
                previousActiveDate.HasValue && lastCompletedDate == previousActiveDate.Value
                    ? (item.DailyStreak ?? 0) + 1
                    : 1;
        }

        private static void ProcessDailyReset(TodoItem item, DateTime today)
        {
            var previousActiveDay = GetPreviousActiveDate(item.ActiveDays, today);
            var lastCompletedDate = item.LastCompletedTimestamp?.Date;

            if (previousActiveDay.HasValue &&
                (!lastCompletedDate.HasValue || lastCompletedDate.Value != previousActiveDay.Value))
            {
                item.DailyStreak = 0;
            }
        }

        private static void ProcessWeeklyReset(TodoItem item, DateTime today)
        {
            var currentWeekStart = StartOfWeek(today);
            var previousWeekStart = currentWeekStart.AddDays(-7);

            var shouldCheckWeekly = item.LastWeeklyCheck == null
                || item.LastWeeklyCheck.Value.Date < currentWeekStart;

            if (!shouldCheckWeekly)
            {
                return;
            }

            var lastCompletedDate = item.LastCompletedTimestamp?.Date;

            var completedInPreviousWeek =
                lastCompletedDate.HasValue &&
                lastCompletedDate.Value >= previousWeekStart &&
                lastCompletedDate.Value < currentWeekStart;

            item.WeeklyStreak = completedInPreviousWeek
                ? (item.WeeklyStreak ?? 0) + 1
                : 0;

            item.LastWeeklyCheck = today;
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }
}
