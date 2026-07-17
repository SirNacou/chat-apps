using System.Security.Claims;

namespace Server.Common.Extensions;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal user)
    {
        public Option<string> GetUserId()
        {
            return user.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}