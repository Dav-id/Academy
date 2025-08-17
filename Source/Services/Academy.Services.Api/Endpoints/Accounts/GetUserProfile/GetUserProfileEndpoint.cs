using Academy.Services.Api.Extensions;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Accounts.GetUserProfile.GetUserProfileContracts;

namespace Academy.Services.Api.Endpoints.Accounts.GetUserProfile
{
    public static class GetUserProfileEndpoint
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/users/{id}", GetProfile);

            // Log mapped routes
            Routes.Add("GET: /{tenant}/api/v1/users/{id}");
        }

        private static async Task<Results<Ok<GetUserProfileResponse>, BadRequest<ErrorResponse>>> GetProfile(string tenant,
            long id,
            ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext db)
        {
            // Create a new logger instance for this endpoint, use the full name of the endpoint class for better logging context
            ILogger logger = loggerFactory.CreateLogger(typeof(GetUserProfileEndpoint).FullName ?? nameof(GetUserProfileEndpoint));

            // Check if the user is authenticated - should be, otherwise this endpoint should not be accessible
            if (httpContextAccessor.HttpContext?.User?.Identity is not ClaimsIdentity cp)
            {
                logger.LogError("GetProfile called without an authenticated user");
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "User is not authenticated.",
                        "Ensure that the user is authenticated before making this request.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            // Check if the user has the required roles or is the same user
            if (!cp.IsInRole("Administrator") && !cp.IsInRole("Instructor") && cp.GetUserId() != id)
            {
                logger.LogError("GetProfile called by user without sufficient permissions. UserId: {UserId}, RequestedId: {RequestedId}", cp.GetUserId(), id);
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status403Forbidden,
                        "Forbidden",
                        "You do not have permission to access this resource.",
                        "Ensure that you have the necessary permissions to access this user profile.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            // Validate the Id parameter
            if (id <= 0)
            {
                logger.LogError("GetProfile called with empty or null Id");

                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status400BadRequest,
                        "Invalid Request",
                        "The Id cannot be null or empty.",
                        "Ensure that the Id is provided in the request body.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            // Check if the user profile exists in the database
            var up = await db.UserProfiles.FirstOrDefaultAsync(x => x.Id == id);
            if (up == null)
            {
                logger.LogError("GetProfile failed to find user profile with Id: {Id}", id);
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status404NotFound,
                        "User Not Found",
                        $"No user profile found with Id: {id}",
                        null,
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            // Return user profile
            return TypedResults.Ok<GetUserProfileResponse>(new(up.Id, up.FirstName, up.LastName, up.Email, up.IsDeleted));
        }
    }
}
