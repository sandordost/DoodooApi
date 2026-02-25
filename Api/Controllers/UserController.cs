using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoodooApi.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController : Controller
    {
        [HttpGet("WhoAmI")]
        public async Task<IActionResult> WhoAmI()
        {
            var userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new { error = "User ID claim not found." });
            }
            return Ok(new { userId });
        }

        [HttpGet("debug/headers")]
        public IActionResult DebugHeaders()
        {
            return Ok(Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()));
        }
    }
}