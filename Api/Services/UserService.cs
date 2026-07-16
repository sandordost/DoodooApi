using Doodoo.SharedKernel.Abstractions;
using DoodooApi.Models.Main.Users;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DoodooApi.Services
{
    public class UserService(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager)
        : ICurrentUser, IUserResetStore, IUserDirectory
    {
        public Guid? GetCurrentUserId()
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var raw =
                user.Claims.FirstOrDefault(c => c.Type == "id")?.Value
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue("sub");

            return Guid.TryParse(raw, out var id) ? id : null;
        }

        public Guid GetCurrentUserIdOrThrow()
        {
            var id = GetCurrentUserId();
            if (id == null) throw new UnauthorizedAccessException("Invalid or missing user id claim.");
            return id.Value;
        }

        public async Task<AppUser?> GetCurrentUserAsync()
        {
            var id = GetCurrentUserId();
            if (id == null) return null;

            return await userManager.FindByIdAsync(id.Value.ToString());
        }

        public async Task<bool> UpdateUserAsync(AppUser user)
        {
            await userManager.UpdateAsync(user);
            return true;
        }

        // IUserResetStore: exposes the AppUser daily/weekly reset markers to the Todos module
        // through the shared abstraction, without leaking AppUser across the module boundary.
        public async Task<UserResetState?> GetAsync(Guid userId)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            return user == null ? null : new UserResetState(user.LastDailyReset, user.LastWeeklyReset);
        }

        public async Task SetAsync(Guid userId, DateTime? lastDailyReset, DateTime? lastWeeklyReset)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user == null) return;

            user.LastDailyReset = lastDailyReset;
            user.LastWeeklyReset = lastWeeklyReset;
            await userManager.UpdateAsync(user);
        }

        // IUserDirectory: lets modules resolve a user id from an email without touching AppUser.
        public async Task<Guid?> FindUserIdByEmailAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            return user?.Id;
        }
    }
}