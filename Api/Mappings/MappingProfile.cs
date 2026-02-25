using AutoMapper;
using DoodooApi.Models.TodoItems;
using DoodooApi.Services;

namespace DoodooApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TodoItemService.CreateTodoItemRequest, TodoItem>();
        }
    }
}