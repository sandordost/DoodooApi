using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.TodoItems;
using DoodooApi.Models.Mappings;
using DoodooApi.Models.Requests.TodoItems;
using DoodooApi.Models.Requests.Transactions;
using DoodooApi.Models.Responses.Transactions;
using Microsoft.EntityFrameworkCore;
using static DoodooApi.Helpers.ActiveDaysHelper;

namespace DoodooApi.Services
{
    public class TodoItemService(AppDbContext context, TransactionService transactionService, CurrencyAccountService currencyAccountService, DifficultyRewardService difficultyRewardService)
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
        public async Task<TransactionProcessResponse> CompleteItemAsync(Guid itemId, Guid userId)
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

            var currencyAccount = await currencyAccountService.GetCurrencyAccountAsync(userId);
            if (currencyAccount == null)
                return new() { ResponseCode = TransactionResponseCode.CurrencyAccountNotFound };

            var rewards = await difficultyRewardService.GetRewardsByDifficultyAsync(
                item.ItemDifficulty,
                userId,
                itemId,
                completedAt
            );

            var transactionRequest = new CreateTransactionRequest
            {
                CurrencyAccountId = currencyAccount.Id,
                SourceType = TransactionSourceType.ItemCompletion,
                SourceIdGuid = item.Id,
                TransactionRecords = [.. rewards.Select(reward => new TransactionRecordRequest
        {
            CurrencyType = reward.CurrencyType,
            Value = reward.Value
        })],
            };

            var transactionResponse = await transactionService.MakeTransactionAsync(transactionRequest);

            if (transactionResponse.ResponseCode != TransactionResponseCode.Created)
                return new() { ResponseCode = transactionResponse.ResponseCode };

            var previousActiveDate = GetPreviousActiveDate(item.ActiveDays, today);
            var lastCompletedDate = item.LastCompletedTimestamp?.Date;

            item.DailyStreak =
                previousActiveDate.HasValue && lastCompletedDate == previousActiveDate.Value
                    ? (item.DailyStreak ?? 0) + 1
                    : 1;

            item.PreviousCompletedTimestamp = item.LastCompletedTimestamp;
            item.CompletedTimestamp = completedAt;
            item.LastCompletedTimestamp = completedAt;

            await context.SaveChangesAsync();

            return new()
            {
                TransactionId = transactionResponse.TransactionId,
                ResponseCode = TransactionResponseCode.Completed
            };
        }

        public async Task<TransactionProcessResponse> UndoCompletionAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return new TransactionProcessResponse { ResponseCode = TransactionResponseCode.ItemNotFound };

            if (item.CompletedTimestamp == null)
            {
                return new TransactionProcessResponse { ResponseCode = TransactionResponseCode.AlreadyReverted };
            }

            // Get the transaction associated with completing this item
            var transaction = await transactionService.GetTransactionBySourceAsync(TransactionSourceType.ItemCompletion, item.Id);

            if (transaction == null) return new TransactionProcessResponse { ResponseCode = TransactionResponseCode.NoTransactionFound };

            // Undo transaction
            var undoResponseCode = await transactionService.UndoTransactionAsync(transaction.Id);
            if (undoResponseCode != TransactionResponseCode.Deleted)
                return new TransactionProcessResponse { ResponseCode = undoResponseCode };

            // Decrease streak count
            item.DailyStreak -= 1;

            item.CompletedTimestamp = null;
            item.LastCompletedTimestamp = item.PreviousCompletedTimestamp;
            await context.SaveChangesAsync();

            return new TransactionProcessResponse { ResponseCode = TransactionResponseCode.Reverted };
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