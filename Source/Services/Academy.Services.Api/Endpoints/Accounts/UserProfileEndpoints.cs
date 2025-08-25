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
            Routes.Add("GET: /{tenant}/api/v1/users?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/users/{id}", GetUserProfile)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/users/{id}");

            app.MapPost("/{tenant}/api/v1/users", CreateUserProfile)
                .Validate<RouteHandlerBuilder, CreateUserProfileRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization(); // was .RequireAuthorization("Instructor")
            Routes.Add("POST: /{tenant}/api/v1/users");

            app.MapPost("/{tenant}/api/v1/users/{id}", UpdateUserProfile)
                .Validate<RouteHandlerBuilder, UpdateUserProfileRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/users/{id}");

            app.MapDelete("/{tenant}/api/v1/users/{id}", DeleteUserProfile)
                .RequireAuthorization(); // was .RequireAuthorization("Instructor")
            Routes.Add("DELETE: /{tenant}/api/v1/users/{id}");
        }

        /// <summary>
        /// Retrieves a list of all user profiles for the current tenant.
        /// </summary>
        /// <param name="tenant">The tenant identifier.</param>
        /// <param name="db">The application database context.</param>
        /// <returns>A list of user profiles.</returns>
        private static async Task<Results<Ok<ListUserProfilesResponse>, BadRequest<ErrorResponse>>> GetUserProfiles(
            string tenant,
            ApplicationDbContext db,
            int page = 1,
            int pageSize = 20)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 20;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            int totalCount = await db.UserProfiles.AsNoTracking().CountAsync();

            IQueryable<UserProfileResponse> query = db.UserProfiles
                .AsNoTracking()
                .OrderBy(x => x.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserProfileResponse(u.Id, u.FirstName, u.LastName, u.Email, !u.IsDeleted));

            List<UserProfileResponse> users = await query.ToListAsync();

            return TypedResults.Ok(new ListUserProfilesResponse(users, totalCount));
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
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            UserProfileResponse? user = await db.UserProfiles
                .AsNoTracking()
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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
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
            string tenant,
            CreateUserProfileRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to create user profiles.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Accounts.UserProfile userProfile = new()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.UserProfiles.Add(userProfile);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new UserProfileResponse(userProfile.Id, userProfile.FirstName, userProfile.LastName, userProfile.Email, !userProfile.IsDeleted));
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
            string tenant,
            long id,
            UpdateUserProfileRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            System.Security.Claims.ClaimsPrincipal? user = httpContext?.User;

            // Get current user ID (assuming it's stored as a claim named "Id")
            string? userIdClaim = user?.FindFirst("Id")?.Value;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;

            if (!long.TryParse(userIdClaim, out long currentUserId))
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "Could not determine current user.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Only allow if updating own profile or user is Instructor
            if (currentUserId != id && !isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to update this user profile.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            if (id != request.Id)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Accounts.UserProfile? userProfile = await db.UserProfiles.FindAsync(id);
            if (userProfile == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"User with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            userProfile.FirstName = request.FirstName;
            userProfile.LastName = request.LastName;
            userProfile.Email = request.Email;
            userProfile.UpdatedBy = httpContext?.User?.Identity?.Name ?? "Unknown";
            userProfile.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new UserProfileResponse(userProfile.Id, userProfile.FirstName, userProfile.LastName, userProfile.Email, !userProfile.IsDeleted));
        }

        /// <summary>
        /// Soft-deletes a user profile by marking it as deleted.
        /// </summary>
        /// <param name="id">The user profile ID.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>Ok if deleted; otherwise, a 404 error.</returns>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteUserProfile(
            string tenant,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to delete user profiles.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Accounts.UserProfile? userProfile = await db.UserProfiles.FindAsync(id);
            if (userProfile == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"User with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            userProfile.IsDeleted = true;
            userProfile.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            userProfile.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}