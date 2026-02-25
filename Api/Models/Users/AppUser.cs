using DoodooApi.Models.TodoItems;
using Microsoft.AspNetCore.Identity;

namespace DoodooApi.Models.Users
{
    public class AppUser : IdentityUser<Guid>
    {
        public List<TodoItem> TodoItems { get; set; } = [];
    }
}
