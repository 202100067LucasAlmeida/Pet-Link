using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PetLink.Controllers
{
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Extracts current authenticated user ID from claims with validation
        /// Returns true if successful, false otherwise
        /// </summary>
        protected bool GetCurrentUserId(out int userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out userId))
            {
                return false;
            }
            return true;
        }
    }
}

