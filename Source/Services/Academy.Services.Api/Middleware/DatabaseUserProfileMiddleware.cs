using Academy.Shared.Data.Contexts;
using Academy.Shared.Security;

using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

namespace Academy.Services.Api.Middleware
{
    /// <summary>
    /// Middleware to load user profile from the database and update the claims in the HttpContext.User.
    /// </summary>
    public class DatabaseUserProfileMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db, IAuthClient authClient)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                string? userId = context.User.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    //Load user profile from DB
                    Academy.Shared.Data.Models.Accounts.UserProfile? userProfile = await db.UserProfiles.FirstOrDefaultAsync(x => x.IdentityProvider == authClient.ProviderName && x.IdentityProviderId == userId);

                    if (userProfile != null)
                    {
                        foreach (Claim claim in context.User.Claims)
                        {
                            // Remove existing claims that we will replace
                            if (claim.Type == "FirstName" || claim.Type == "LastName" || claim.Type == ClaimTypes.Email || claim.Type == "Id" || claim.Type == "IdentityProvider" || claim.Type == "IdentityProviderId")
                            {
                                ((ClaimsIdentity)context.User.Identity).RemoveClaim(claim);
                            }
                        }

                        List<Claim> claims = [
                            new ("Id", userProfile.Id.ToString()),
                            new ("IdentityProvider", userProfile.IdentityProvider),
                            new ("IdentityProviderId", userProfile.IdentityProviderId),
                            new ("FirstName", userProfile.FirstName),
                            new ("LastName", userProfile.LastName),
                            new ("FullName", userProfile.FirstName + (!string.IsNullOrEmpty(userProfile.FirstName) ? " " : "") + userProfile.LastName),
                            new (ClaimTypes.Email, userProfile.Email),
                        ];

                        claims.AddRange(context.User.Claims);

                        // Create a new identity and principal
                        ClaimsIdentity newIdentity = new(claims: claims, context.User.Identity.AuthenticationType, nameType: "id", roleType: "roles");
                        // Replace the current user
                        context.User = new(newIdentity);
                    }
                }
            }

            await next(context);
        }
    }
}
