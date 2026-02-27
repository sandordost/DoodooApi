using AutoMapper;
using DoodooApi.Models;
using DoodooApi.Models.Rewards;
using DoodooApi.Models.TodoItems;
using DoodooApi.Services;

namespace DoodooApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TodoItemService.CreateTodoItemRequest, TodoItem>();
            CreateMap<RewardService.CreateRewardRequest, Reward>();
            CreateMap<TransactionService.CreateTransactionRequest, Transaction>();
        }
    }
}