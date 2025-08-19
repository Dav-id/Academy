using System.Security.Claims;

namespace Academy.Services.Api.Extensions
{
    // add GetUserId extension method to IdentityUser
    public static class ClaimsPrincipalExtensions
    {

        private static Claim GetClaim(ClaimsIdentity user, string claim)
        {
            ArgumentNullException.ThrowIfNull(user);

            Claim? userClaim = user.FindFirst(claim);
            if (string.IsNullOrEmpty(userClaim?.Value))
            {
                throw new InvalidOperationException($"{claim} claim not found.");
            }

            return userClaim;
        }

        public static long? GetUserId(this ClaimsPrincipal user)
        {
            Claim? idClaim = user.FindFirst("Id") ?? user.FindFirst(ClaimTypes.NameIdentifier);
            if (idClaim != null && long.TryParse(idClaim.Value, out long id))
                return id;
            return null;
        }

        public static long GetUserId(this ClaimsIdentity user)
        {
            Claim claim = GetClaim(user, "id");

            if (!long.TryParse(claim.Value, out long id))
            {
                throw new InvalidOperationException("User ID claim is not a valid long.");
            }

            return id;
        }

        public static string GetUserName(this ClaimsIdentity user)
        {
            Claim claim = GetClaim(user, ClaimTypes.Name);

            return claim?.Value ?? string.Empty;
        }

        public static string GetEmail(this ClaimsIdentity user)
        {
            Claim claim = GetClaim(user, ClaimTypes.Email);

            return claim?.Value ?? string.Empty;
        }

        public static bool IsInRole(this ClaimsIdentity user, string role)
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
