using AutoMapper;
using DoodooApi.Models.Database;
using DoodooApi.Models.Enums;
using DoodooApi.Models.TodoItems;
using Microsoft.EntityFrameworkCore;

namespace DoodooApi.Services
{
    public class TodoItemService(AppDbContext context, IMapper mapper)
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

        public async Task<bool> CompleteItemAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return false;

            if (item.CompletedTimestamp == null)
            {
                item.CompletedTimestamp = DateTime.UtcNow;
                await context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> UndoCompletionAsync(Guid itemId, Guid userId)
        {
            var item = await context.TodoItems
                .FirstOrDefaultAsync(i => i.Id == itemId && i.OwnerId == userId && i.DeletedTimestamp == null);

            if (item == null) return false;

            if (item.CompletedTimestamp != null)
            {
                item.CompletedTimestamp = null;
                await context.SaveChangesAsync();
            }

            return true;
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

        public record CreateTodoItemRequest
        {
            public required string Title { get; set; }
            public string? Description { get; set; }
            public ItemDifficulty ItemDifficulty { get; set; } = ItemDifficulty.Easy;
            public ItemCategory ItemCategory { get; set; }
        }

        public async Task<TodoItem> CreateItemAsync(Guid userId, CreateTodoItemRequest request)
        {
            var newItem = mapper.Map<TodoItem>(request);

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