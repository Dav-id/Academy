using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Lessons.LessonPrerequisiteAssessmentContracts;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Provides API endpoints for managing lesson prerequisite assessments.
    /// </summary>
    public static class LessonPrerequisiteAssessmentEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/lessons/{lessonId}/prerequisite-assessments", GetLessonPrerequisiteAssessments)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/lessons/{lessonId}/prerequisite-assessments?page={page}&pageSize={pageSize}");

            app.MapPost("/{tenant}/api/v1/lessons/{lessonId}/prerequisite-assessments", CreateLessonPrerequisiteAssessment)
                .Validate<RouteHandlerBuilder, CreateLessonPrerequisiteAssessmentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/lessons/{lessonId}/prerequisite-assessments");

            app.MapDelete("/{tenant}/api/v1/lessons/{lessonId}/prerequisite-assessments/{prerequisiteAssessmentId}", DeleteLessonPrerequisiteAssessment)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/lessons/{lessonId}/prerequisite-assessments/{prerequisiteAssessmentId}");
        }

        /// <summary>
        /// Gets all prerequisite assessments for a lesson.
        /// </summary>
        private static async Task<Results<Ok<ListLessonPrerequisiteAssessmentsResponse>, BadRequest<ErrorResponse>>> GetLessonPrerequisiteAssessments(
            string tenant,
            long lessonId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            int page = 1,
            int pageSize = 20)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            bool isInstructor = ((user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false));
            long? userId = user?.GetUserId();

            // Get the courseId for this lesson
            Shared.Data.Models.Lessons.Lesson? lesson = await db.Lessons
                .Include(l => l.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson with Id {lessonId} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            long? courseId = lesson.CourseModule?.CourseId;

            bool hasAccess = isInstructor ||
                (userId.HasValue && courseId.HasValue &&
                 await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value));

            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this lesson's prerequisite assessments.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            int totalCount = await db.LessonPrerequisiteAssessments
                .AsNoTracking()
                .Where(p => p.LessonId == lessonId)
                .CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<LessonPrerequisiteAssessmentResponse> prerequisites = await db.LessonPrerequisiteAssessments
                .AsNoTracking()
                .Where(p => p.LessonId == lessonId)
                .OrderBy(p => p.PrerequisiteAssessmentId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new LessonPrerequisiteAssessmentResponse(p.LessonId, p.PrerequisiteAssessmentId ?? 0))
                .ToListAsync();

            return TypedResults.Ok(new ListLessonPrerequisiteAssessmentsResponse(prerequisites, totalCount));
        }

        /// <summary>
        /// Creates a new prerequisite assessment for a lesson.
        /// </summary>
        private static async Task<Results<Ok<LessonPrerequisiteAssessmentResponse>, BadRequest<ErrorResponse>>> CreateLessonPrerequisiteAssessment(
            string tenant,
            long lessonId,
            CreateLessonPrerequisiteAssessmentRequest request,
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
                    "You are not allowed to create lesson prerequisite assessments.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Prevent duplicate prerequisites
            bool exists = await db.LessonPrerequisiteAssessments
                .AnyAsync(p => p.LessonId == lessonId && p.PrerequisiteAssessmentId == request.PrerequisiteAssessmentId);
            if (exists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    "This prerequisite assessment already exists for the lesson.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.LessonPrerequisiteAssessment prerequisite = new()
            {
                LessonId = lessonId,
                PrerequisiteAssessmentId = request.PrerequisiteAssessmentId
            };

            db.LessonPrerequisiteAssessments.Add(prerequisite);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new LessonPrerequisiteAssessmentResponse(prerequisite.LessonId, prerequisite.PrerequisiteAssessmentId ?? 0));
        }

        /// <summary>
        /// Deletes a prerequisite assessment from a lesson.
        /// </summary>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteLessonPrerequisiteAssessment(
            string tenant,
            long lessonId,
            long prerequisiteAssessmentId,
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
                    "You are not allowed to delete lesson prerequisite assessments.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.LessonPrerequisiteAssessment? prerequisite = await db.LessonPrerequisiteAssessments
                .FirstOrDefaultAsync(p => p.PrerequisiteAssessmentId == prerequisiteAssessmentId && p.LessonId == lessonId);
            if (prerequisite == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Prerequisite assessment with Id {prerequisiteAssessmentId} not found for lesson {lessonId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            db.LessonPrerequisiteAssessments.Remove(prerequisite);
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}