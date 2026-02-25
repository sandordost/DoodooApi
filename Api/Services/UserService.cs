using DoodooApi.Models.Users;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace DoodooApi.Services
{
    public class UserService(IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager)
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
    }
}