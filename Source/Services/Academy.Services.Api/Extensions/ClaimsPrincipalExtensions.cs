using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;

namespace Academy.Services.Api.Extensions
{

    // add GetUserId extension method to IdentityUser
    public static class ClaimsPrincipalExtensions
    {

        private static Claim GetClaim(CaseSensitiveClaimsIdentity user, string claim)
        {
            ArgumentNullException.ThrowIfNull(user);

            Claim userClaim = user.FindFirst(claim);
            if (string.IsNullOrEmpty(userClaim?.Value))
            {
                throw new InvalidOperationException($"{claim} claim not found.");
            }

            return userClaim;
        }

        public static string GetUserId(this CaseSensitiveClaimsIdentity user)
        {
            Claim claim = GetClaim(user, ClaimTypes.NameIdentifier);

            return claim?.Value ?? string.Empty;
        }


        public static string GetUserName(this CaseSensitiveClaimsIdentity user)
        {
            Claim claim = GetClaim(user, ClaimTypes.Name);

            return claim?.Value ?? string.Empty;
        }

        public static string GetEmail(this CaseSensitiveClaimsIdentity user)
        {
            Claim claim = GetClaim(user, ClaimTypes.Email);

            return claim?.Value ?? string.Empty;
        }

        public static bool IsInRole(this CaseSensitiveClaimsIdentity user, string role)
        {
            ArgumentNullException.ThrowIfNull(user);

            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("Role cannot be null or empty.", nameof(role));
            }

            return user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value.Equals(role, StringComparison.OrdinalIgnoreCase));
        }
    }
}
