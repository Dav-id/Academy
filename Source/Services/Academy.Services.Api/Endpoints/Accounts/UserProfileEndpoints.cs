using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Accounts.UserProfileContracts;

namespace Academy.Services.Api.Endpoints.Accounts
{
    /// <summary>
    /// Provides API endpoints for managing user profiles (accounts).
    /// </summary>
    public static class UserProfileEndpoints
    {
        public static readonly List<string> Routes = [];

        /// <summary>
        /// Registers user profile endpoints for CRUD operations.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/users", GetUserProfiles)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/users");

            app.MapGet("/{tenant}/api/v1/users/{id}", GetUserProfile)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/users/{{id}}");

            app.MapPost("/{tenant}/api/v1/users", CreateUserProfile)
                .Validate<RouteHandlerBuilder, CreateUserProfileRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/users");

            app.MapPut("/{tenant}/api/v1/users/{id}", UpdateUserProfile)
                .Validate<RouteHandlerBuilder, UpdateUserProfileRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"PUT: /{{tenant}}/api/v1/users/{{id}}");

            app.MapDelete("/{tenant}/api/v1/users/{id}", DeleteUserProfile)
                .RequireAuthorization("Instructor");
            Routes.Add($"DELETE: /{{tenant}}/api/v1/users/{{id}}");
        }

        /// <summary>
        /// Retrieves a list of all user profiles for the current tenant.
        /// </summary>
        /// <param name="tenant">The tenant identifier.</param>
        /// <param name="db">The application database context.</param>
        /// <returns>A list of user profiles.</returns>
        private static async Task<Results<Ok<ListUserProfilesResponse>, BadRequest<ErrorResponse>>> GetUserProfiles(
            string tenant,
            ApplicationDbContext db)
        {
            List<UserProfileResponse> users = await db.UserProfiles
                .Select(u => new UserProfileResponse(u.Id, u.FirstName, u.LastName, u.Email, !u.IsDeleted))
                .ToListAsync();

            return TypedResults.Ok(new ListUserProfilesResponse(users));
        }

        /// <summary>
        /// Retrieves a specific user profile by ID.
        /// </summary>
        /// <param name="tenant">The tenant identifier.</param>
        /// <param name="id">The user profile ID.</param>
        /// <param name="db">The application database context.</param>
        /// <returns>The user profile if found; otherwise, a 404 error.</returns>
        private static async Task<Results<Ok<UserProfileResponse>, BadRequest<ErrorResponse>>> GetUserProfile(
            string tenant,
            long id,
            ApplicationDbContext db)
        {
            UserProfileResponse? user = await db.UserProfiles
                .Where(u => u.Id == id)
                .Select(u => new UserProfileResponse(u.Id, u.FirstName, u.LastName, u.Email, !u.IsDeleted))
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"User with Id {id} not found.",
                    null,
                    null
                ));
            }

            return TypedResults.Ok(user);
        }

        /// <summary>
        /// Creates a new user profile.
        /// </summary>
        /// <param name="request">The user profile creation request.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>The created user profile.</returns>
        private static async Task<Results<Ok<UserProfileResponse>, BadRequest<ErrorResponse>>> CreateUserProfile(
            CreateUserProfileRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Accounts.UserProfile user = new()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.UserProfiles.Add(user);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new UserProfileResponse(user.Id, user.FirstName, user.LastName, user.Email, !user.IsDeleted));
        }

        /// <summary>
        /// Updates an existing user profile.
        /// </summary>
        /// <param name="id">The user profile ID (from route).</param>
        /// <param name="request">The update request.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>The updated user profile if found; otherwise, a 404 error.</returns>
        private static async Task<Results<Ok<UserProfileResponse>, BadRequest<ErrorResponse>>> UpdateUserProfile(
            long id,
            UpdateUserProfileRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            if (id != request.Id)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    null
                ));
            }

            Shared.Data.Models.Accounts.UserProfile? user = await db.UserProfiles.FindAsync(id);
            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"User with Id {id} not found.",
                    null,
                    null
                ));
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            user.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new UserProfileResponse(user.Id, user.FirstName, user.LastName, user.Email, !user.IsDeleted));
        }

        /// <summary>
        /// Soft-deletes a user profile by marking it as deleted.
        /// </summary>
        /// <param name="id">The user profile ID.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>Ok if deleted; otherwise, a 404 error.</returns>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteUserProfile(
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Accounts.UserProfile? user = await db.UserProfiles.FindAsync(id);
            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"User with Id {id} not found.",
                    null,
                    null
                ));
            }

            user.IsDeleted = true;
            user.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            user.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}