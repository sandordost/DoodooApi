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
            return await context.TodoItems.Where(i => i.OwnerId == userId && i.DeletedTimestamp == null).ToListAsync();
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

            if (item.CompletedTimestamp != null)
                return new() { ResponseCode = TransactionResponseCode.AlreadyCompleted };

            var completedAt = DateTime.UtcNow;
            var today = completedAt.Date;

            if (!IsActiveOn(item.ActiveDays, today.DayOfWeek))
            {
                if (item.ItemCategory == ItemCategory.Daily)
                {
                    return new() { ResponseCode = TransactionResponseCode.AlreadyCompleted };
                }
            }

            // Currency owns the ledger. Ask it to grant the completion reward (it resolves the
            // account, calls Rewards for the difficulty amounts, and writes the transaction).
            var grant = await bus.InvokeAsync<ItemCompletionRewardResult>(
                new GrantItemCompletionReward(userId, item.Id, item.ItemDifficulty, completedAt));

            if (grant.ResponseCode != TransactionResponseCode.Created)
                return new() { ResponseCode = grant.ResponseCode };

            var previousActiveDate = GetPreviousActiveDate(item.ActiveDays, today);
            var lastCompletedDate = item.LastCompletedTimestamp?.Date;

            item.DailyStreak =
                previousActiveDate.HasValue && lastCompletedDate == previousActiveDate.Value
                    ? (item.DailyStreak ?? 0) + 1
                    : 1;

            item.PreviousCompletedTimestamp = item.LastCompletedTimestamp;
            item.CompletedTimestamp = completedAt;
            item.LastCompletedTimestamp = completedAt;

            try
            {
                await context.SaveChangesAsync();
            }
            catch
            {
                // The grant succeeded but persisting the item failed. Compensate so the user
                // is not paid for a completion that did not stick. The grant is idempotent and
                // the revert is keyed on the same source, so this converges.
                await bus.InvokeAsync<RevertResult>(new RevertItemCompletionReward(userId, item.Id));
                throw;
            }

            return new()
            {
                TransactionId = grant.TransactionId,
                ResponseCode = TransactionResponseCode.Completed
            };
        }

        public async Task<TodoActionResult> UndoCompletionAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return new TodoActionResult { ResponseCode = TransactionResponseCode.ItemNotFound };

            if (item.CompletedTimestamp == null)
            {
                return new TodoActionResult { ResponseCode = TransactionResponseCode.AlreadyReverted };
            }

            // Ask Currency to revert the completion grant (idempotent, keyed on the item id).
            var revert = await bus.InvokeAsync<RevertResult>(
                new RevertItemCompletionReward(userId, item.Id));

            if (revert.ResponseCode != TransactionResponseCode.Deleted)
                return new TodoActionResult { ResponseCode = revert.ResponseCode };

            // Decrease streak count
            item.DailyStreak -= 1;

            item.CompletedTimestamp = null;
            item.LastCompletedTimestamp = item.PreviousCompletedTimestamp;
            await context.SaveChangesAsync();

            return new TodoActionResult { ResponseCode = TransactionResponseCode.Reverted };
        }

        public async Task<bool> DeleteItemAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return false;

            item.DeletedTimestamp = DateTime.UtcNow;
            await context.SaveChangesAsync();
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

            context.TodoItems.Add(newItem);
            await context.SaveChangesAsync();

            return newItem;
        }

        public async Task<TodoItem?> UpdateItemAsync(Guid id, Guid userId, UpdateTodoItemRequest request)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == id && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null)
                return null;

            item.Title = request.Title ?? item.Title;
            item.Description = request.Description ?? item.Description;
            item.ItemDifficulty = request.ItemDifficulty ?? item.ItemDifficulty;
            item.ItemCategory = request.ItemCategory ?? item.ItemCategory;
            item.ActiveDays = request.ActiveDays ?? item.ActiveDays;
            await context.SaveChangesAsync();

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
                // LastCompletedTimestamp is kept for streak calculations.
                if (item.CompletedTimestamp is { } completed && completed.Date < currentWeekStart)
                {
                    item.CompletedTimestamp = null;
                }
            }

            await context.SaveChangesAsync();
        }

        private static void ProcessDailyReset(TodoItem item, DateTime today)
        {
            var previousActiveDay = GetPreviousActiveDate(item.ActiveDays, today);
            var lastCompletedDate = item.LastCompletedTimestamp?.Date;

            if (previousActiveDay.HasValue &&
                 (!lastCompletedDate.HasValue || lastCompletedDate.Value != previousActiveDay.Value)
)
            {
                item.DailyStreak = 0;
            }
        }

        /// <returns><c>true</c> when a new week rolled over and the item was processed.</returns>
        private static bool ProcessWeeklyReset(TodoItem item, DateTime today)
        {
            var currentWeekStart = StartOfWeek(today);
            var previousWeekStart = currentWeekStart.AddDays(-7);

            var shouldCheckWeekly = item.LastWeeklyCheck == null
                || item.LastWeeklyCheck.Value.Date < currentWeekStart;

            if (!shouldCheckWeekly)
            {
                return false;
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
            return true;
        }

        public async Task<bool> SetItemOrder(Guid itemId, Guid userId, int newOrder)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return false;

            item.Order = newOrder;
            await context.SaveChangesAsync();

            return true;
        }

        private static DateTime StartOfWeek(DateTime date)
        {
            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-diff).Date;
        }
    }
}
