using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using DoodooApi.Models.Main.TodoItems;
using DoodooApi.Models.Mappings;
using DoodooApi.Models.Requests.TodoItems;
using DoodooApi.Models.Requests.Transactions;
using DoodooApi.Models.Responses.Transactions;
using Microsoft.EntityFrameworkCore;

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

            // Get currency account for user
            var currencyAccount = await currencyAccountService.GetCurrencyAccountAsync(userId);
            if (currencyAccount == null) return new() { ResponseCode = TransactionResponseCode.CurrencyAccountNotFound };

            // Get reward amount based on item difficulty
            var rewards = await difficultyRewardService.GetRewardsByDifficultyAsync(item.ItemDifficulty);

            var transactionRequest = new CreateTransactionRequest
            {
                CurrencyAccountId = currencyAccount.Id,
                SourceType = TransactionSourceType.ItemCompletion,
                SourceIdGuid = item.Id,
                TransactionRecords = [.. rewards.Select(reward =>
                {
                    return new TransactionRecordRequest
                    {
                        CurrencyType = reward.CurrencyType,
                        Value = reward.Value
                    };
                })],
            };

            // Create reward transaction for completing the item
            var transactionResponse = await transactionService.MakeTransactionAsync(transactionRequest);

            if (transactionResponse.ResponseCode != TransactionResponseCode.Created)
                return new() { ResponseCode = transactionResponse.ResponseCode };

            item.CompletedTimestamp = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return new() { TransactionId = transactionResponse.TransactionId, ResponseCode = TransactionResponseCode.Completed };
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

            item.CompletedTimestamp = null;
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

            context.TodoItems.Add(newItem);
            await context.SaveChangesAsync();

            return newItem;
        }
    }
}