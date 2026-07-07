using Microsoft.AspNetCore.Identity;

namespace DoodooApi.Models.Main.Users
{
    public class AppUser : IdentityUser<Guid>
    {
        // Todos-reset bookkeeping. Kept on the user for now; could later move to a
        // Todos-owned per-user state row.
        public DateTime? LastDailyReset { get; set; }
        public DateTime? LastWeeklyReset { get; set; }
    }
}
