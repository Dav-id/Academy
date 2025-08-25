using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Courses.CourseCompletionContracts;

namespace Academy.Services.Api.Endpoints.Courses
{
    /// <summary>
    /// Provides API endpoints for managing course completions.
    /// </summary>
    public static class CourseCompletionEndpoints
    {

        public static readonly List<string> Routes = [];

        /// <summary>
        /// Registers course completion endpoints.
        /// </summary>
        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/courses/{courseId}/completions", GetCourseCompletions)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/completions?page={page}&pageSize={pageSize}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/complete", SubmitCompletion)
                .Validate<RouteHandlerBuilder, SubmitCompletionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/complete");
        }

        /// <summary>
        /// Submits a course completion for the current user.
        /// </summary>
        public static async Task<Results<Ok<CompletionResponse>, BadRequest<ErrorResponse>>> SubmitCompletion(
            string tenant,
            long courseId,
            SubmitCompletionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to submit course completions.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            long? userId = user?.GetUserId();
            if (userId is null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Get userprofile from the database
            Shared.Data.Models.Accounts.UserProfile? userProfile = await db.UserProfiles.FirstOrDefaultAsync(up => up.Id == request.UserProfileId);
            if (userProfile is null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "User Profile Not Found",
                    "The specified user profile does not exist.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Check if already completed
            Shared.Data.Models.Courses.CourseCompletion? existing = await db.CourseCompletions
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserProfileId == userProfile.Id);

            if (existing != null)
            {
                return TypedResults.Ok(new CompletionResponse(
                    existing.Id, existing.CourseId, existing.UserProfileId, existing.SubmittedOn,
                    existing.IsPassed, existing.FinalScore, existing.Feedback));
            }

            Shared.Data.Models.Courses.CourseCompletion completion = new()
            {
                CourseId = courseId,
                UserProfileId = userProfile.Id,
                SubmittedOn = DateTime.UtcNow,
                IsPassed = request.IsPassed,
                FinalScore = request.FinalScore,
                Feedback = request.Feedback ?? string.Empty,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.CourseCompletions.Add(completion);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new CompletionResponse(
                completion.Id, completion.CourseId, completion.UserProfileId, completion.SubmittedOn,
                completion.IsPassed, completion.FinalScore, completion.Feedback));
        }

        /// <summary>
        /// Gets all completions for a course (instructor only).
        /// </summary>
        public static async Task<Results<Ok<ListCompletionsResponse>, BadRequest<ErrorResponse>>> GetCourseCompletions(
            string tenant,
            long courseId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            int page = 1,
            int pageSize = 20)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to view course completions.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            int totalCount = await db.CourseCompletions
                .AsNoTracking()
                .Where(c => c.CourseId == courseId)
                .CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<CompletionResponse> completions = await db.CourseCompletions
                .AsNoTracking()
                .Where(c => c.CourseId == courseId)
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CompletionResponse(
                    c.Id, c.CourseId, c.UserProfileId, c.SubmittedOn,
                    c.IsPassed, c.FinalScore, c.Feedback))
                .ToListAsync();

            return TypedResults.Ok(new ListCompletionsResponse(completions, totalCount));
        }
    }
}