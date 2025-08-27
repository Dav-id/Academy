using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Lessons.LessonPrerequisiteLessonContracts;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Provides API endpoints for managing lesson prerequisite lessons.
    /// </summary>
    public static class LessonPrerequisiteLessonEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/prerequisites", GetLessonPrerequisites)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/prerequisites?page={page}&pageSize={pageSize}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/prerequisites", CreateLessonPrerequisite)
                .Validate<RouteHandlerBuilder, CreateLessonPrerequisiteLessonRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/prerequisites");

            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/prerequisites/{prerequisiteLessonId}", DeleteLessonPrerequisite)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/prerequisites/{prerequisiteLessonId}");
        }

        /// <summary>
        /// Gets all prerequisite lessons for a lesson.
        /// </summary>
        private static async Task<Results<Ok<ListLessonPrerequisiteLessonsResponse>, BadRequest<ErrorResponse>>> GetLessonPrerequisites(
            string tenant,
            long courseId,
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

            // Get the courseId for this lesson and validate hierarchy
            Shared.Data.Models.Lessons.Lesson? lesson = await db.Lessons
                .Include(l => l.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseModule != null && l.CourseModule.CourseId == courseId);

            if (lesson == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson with Id {lessonId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            long? dbCourseId = lesson.CourseModule?.CourseId;

            bool hasAccess = isInstructor ||
                (userId.HasValue && dbCourseId.HasValue &&
                 await db.CourseEnrollments.AnyAsync(e => e.CourseId == dbCourseId && e.UserProfileId == userId.Value));

            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this lesson's prerequisites.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            int totalCount = await db.LessonPrerequisiteLessons
                .AsNoTracking()
                .Where(p => p.LessonId == lessonId)
                .CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<LessonPrerequisiteLessonResponse> prerequisites = await db.LessonPrerequisiteLessons
                .AsNoTracking()
                .Where(p => p.LessonId == lessonId)
                .OrderBy(p => p.PrerequisiteLessonId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new LessonPrerequisiteLessonResponse(p.LessonId, p.PrerequisiteLessonId ?? 0))
                .ToListAsync();

            return TypedResults.Ok(new ListLessonPrerequisiteLessonsResponse(prerequisites, totalCount));
        }

        /// <summary>
        /// Creates a new prerequisite lesson for a lesson.
        /// </summary>
        private static async Task<Results<Ok<LessonPrerequisiteLessonResponse>, BadRequest<ErrorResponse>>> CreateLessonPrerequisite(
            string tenant,
            long courseId,
            long lessonId,
            CreateLessonPrerequisiteLessonRequest request,
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
                    "You are not allowed to create lesson prerequisites.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Validate lesson belongs to course
            var lesson = await db.Lessons
                .Include(l => l.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseModule != null && l.CourseModule.CourseId == courseId);
            if (lesson == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson with Id {lessonId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Prevent duplicate prerequisites
            bool exists = await db.LessonPrerequisiteLessons
                .AnyAsync(p => p.LessonId == lessonId && p.PrerequisiteLessonId == request.PrerequisiteLessonId);
            if (exists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    "This prerequisite already exists for the lesson.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.LessonPrerequisiteLesson prerequisite = new()
            {
                LessonId = lessonId,
                PrerequisiteLessonId = request.PrerequisiteLessonId
            };

            db.LessonPrerequisiteLessons.Add(prerequisite);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new LessonPrerequisiteLessonResponse(prerequisite.LessonId, prerequisite.PrerequisiteLessonId ?? 0));
        }

        /// <summary>
        /// Deletes a prerequisite lesson from a lesson.
        /// </summary>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteLessonPrerequisite(
            string tenant,
            long courseId,
            long lessonId,
            long prerequisiteLessonId,
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
                    "You are not allowed to delete lesson prerequisites.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Validate lesson belongs to course
            var lesson = await db.Lessons
                .Include(l => l.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == lessonId && l.CourseModule != null && l.CourseModule.CourseId == courseId);
            if (lesson == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson with Id {lessonId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.LessonPrerequisiteLesson? prerequisite = await db.LessonPrerequisiteLessons
                .FirstOrDefaultAsync(p => p.PrerequisiteLessonId == prerequisiteLessonId && p.LessonId == lessonId);
            if (prerequisite == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Prerequisite lesson with Id {prerequisiteLessonId} not found for lesson {lessonId} in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            db.LessonPrerequisiteLessons.Remove(prerequisite);
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}