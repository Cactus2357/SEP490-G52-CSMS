using System.Security.Claims;

namespace SEP490_G52_CSMS.Commons
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetBranchId(this ClaimsPrincipal user)
        {
            return user.FindFirst("BranchId")?.Value;
        }

        public static int? GetEmployeeId(this ClaimsPrincipal user)
        {
            var val = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(val, out int id)) return id;
            return null;
        }

        public static string? GetFullName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string? GetRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value;
        }

        public static string? GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst("Username")?.Value;
        }
    }
}
