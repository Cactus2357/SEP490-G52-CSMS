using System.Security.Claims;

namespace SEP490_G52_CSMS.Commons
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetBranchId(this ClaimsPrincipal user)
        {
            return user.FindFirst("BranchId")?.Value;
        }
    }
}
