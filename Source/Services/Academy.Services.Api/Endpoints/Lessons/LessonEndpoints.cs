using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Lessons.LessonContracts;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Provides API endpoints for managing lessons.
    /// </summary>
    public static class LessonEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons", GetLessons)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons/{id}", GetLesson)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons/{id}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons", CreateLesson)
                .Validate<RouteHandlerBuilder, CreateLessonRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons");

            // Update uses POST as it expects the full entity in the body, not just the fields to update.
            app.MapPost("/{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons/{id}", UpdateLesson)
                .Validate<RouteHandlerBuilder, UpdateLessonRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons/{id}");

            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons/{id}", DeleteLesson)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{courseId}/modules/{moduleId}/lessons/{id}");
        }

        /// <summary>
        /// Gets all lessons for a module, if the user has access to the course.
        /// </summary>
        public static async Task<Results<Ok<ListLessonsResponse>, BadRequest<ErrorResponse>>> GetLessons(
            string tenant,
            long courseId,
            long moduleId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            int page = 1,
            int pageSize = 20)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            long? userId = user?.GetUserId();

            // Only show lessons if user is instructor or enrolled in the course
            Shared.Data.Models.Courses.CourseModule? module = await db.CourseModules
                .Include(m => m.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId);
            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {moduleId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            bool hasAccess = isInstructor ||
                userId.HasValue && await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value);
            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this module.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            int totalCount = await db.Lessons
                .AsNoTracking()
                .Where(l => l.CourseModuleId == moduleId)
                .CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<LessonResponse> lessons = await db.Lessons
                .AsNoTracking()
                .Where(l => l.CourseModuleId == moduleId)
                .OrderBy(l => l.Order)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LessonResponse(l.Id, l.CourseModuleId, l.Title, l.Summary, l.Order, l.AvailableFrom, l.AvailableTo))
                .ToListAsync();

            return TypedResults.Ok(new ListLessonsResponse(lessons, totalCount));
        }

        /// <summary>
        /// Gets a specific lesson for a module, if the user has access.
        /// </summary>
        public static async Task<Results<Ok<LessonResponse>, BadRequest<ErrorResponse>>> GetLesson(
            string tenant,
            long courseId,
            long moduleId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            long? userId = user?.GetUserId();

            Shared.Data.Models.Courses.CourseModule? module = await db.CourseModules
                .Include(m => m.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId);
            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {moduleId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            bool hasAccess = isInstructor ||
                userId.HasValue && await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value);
            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this module.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            LessonResponse? lesson = await db.Lessons
                .AsNoTracking()
                .Where(l => l.CourseModuleId == moduleId && l.Id == id)
                .Select(l => new LessonResponse(l.Id, l.CourseModuleId, l.Title, l.Summary, l.Order, l.AvailableFrom, l.AvailableTo))
                .FirstOrDefaultAsync();

            if (lesson == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson with Id {id} not found in module {moduleId} and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(lesson);
        }

        /// <summary>
        /// Creates a new lesson for a module.
        /// </summary>
        public static async Task<Results<Ok<LessonResponse>, BadRequest<ErrorResponse>>> CreateLesson(
            string tenant,
            long courseId,
            long moduleId,
            CreateLessonRequest request,
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
                    "You are not allowed to create lessons.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Validate module belongs to course
            var module = await db.CourseModules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId);
            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {moduleId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.Lesson lesson = new()
            {
                CourseModuleId = moduleId,
                Title = request.Title,
                Summary = request.Summary,
                Order = request.Order,
                AvailableFrom = request.AvailableFrom,
                AvailableTo = request.AvailableTo,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.Lessons.Add(lesson);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new LessonResponse(lesson.Id, lesson.CourseModuleId, lesson.Title, lesson.Summary, lesson.Order, lesson.AvailableFrom, lesson.AvailableTo));
        }

        /// <summary>
        /// Updates an existing lesson for a module.
        /// </summary>
        public static async Task<Results<Ok<LessonResponse>, BadRequest<ErrorResponse>>> UpdateLesson(
            string tenant,
            long courseId,
            long moduleId,
            long id,
            UpdateLessonRequest request,
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
                    "You are not allowed to update lessons.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            if (id != request.Id || moduleId != request.CourseModuleId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Validate module belongs to course
            var module = await db.CourseModules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId);
            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {moduleId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.Lesson? lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id && l.CourseModuleId == moduleId);
            if (lesson == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson with Id {id} not found in module {moduleId} and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            lesson.Title = request.Title;
            lesson.Summary = request.Summary;
            lesson.Order = request.Order;
            lesson.AvailableFrom = request.AvailableFrom;
            lesson.AvailableTo = request.AvailableTo;
            lesson.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            lesson.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new LessonResponse(lesson.Id, lesson.CourseModuleId, lesson.Title, lesson.Summary, lesson.Order, lesson.AvailableFrom, lesson.AvailableTo));
        }

        /// <summary>
        /// Soft-deletes a lesson by marking it as deleted.
        /// </summary>
        public static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteLesson(
            string tenant,
            long courseId,
            long moduleId,
            long id,
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
                    "You are not allowed to delete lessons.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Validate module belongs to course
            var module = await db.CourseModules
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId);
            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {moduleId} not found in course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.Lesson? lesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == id && l.CourseModuleId == moduleId);
            if (lesson == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson with Id {id} not found in module {moduleId} and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            lesson.IsDeleted = true;
            lesson.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            lesson.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}