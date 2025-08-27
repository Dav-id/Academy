using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Lessons.LessonSectionContracts;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Provides API endpoints for managing lesson sections.
    /// </summary>
    public static class LessonSectionEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections", GetLessonSections)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections/{id}", GetLessonSection)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections/{id}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections", CreateLessonSection)
                .Validate<RouteHandlerBuilder, CreateLessonSectionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections/{id}", UpdateLessonSection)
                .Validate<RouteHandlerBuilder, UpdateLessonSectionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections/{id}");

            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections/{id}", DeleteLessonSection)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{courseId}/lessons/{lessonId}/lessonsections/{id}");
        }


        /// <summary>
        /// Gets all sections for a lesson.
        /// </summary>
        public static async Task<Results<Ok<ListLessonSectionsResponse>, BadRequest<ErrorResponse>>> GetLessonSections(
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

            // Validate lesson and course
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

            long? dbCourseId = lesson.CourseModule?.CourseId;

            bool hasAccess = isInstructor ||
                (userId.HasValue && dbCourseId.HasValue &&
                 await db.CourseEnrollments.AnyAsync(e => e.CourseId == dbCourseId && e.UserProfileId == userId.Value));

            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this lesson's sections.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            int totalCount = await db.LessonSections
                .AsNoTracking()
                .Where(s => s.LessonId == lessonId)
                .CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<LessonSectionResponse> sections = await db.LessonSections
                .AsNoTracking()
                .Where(s => s.LessonId == lessonId)
                .OrderBy(s => s.Order)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new LessonSectionResponse(s.Id, s.LessonId, s.Description, s.Order))
                .ToListAsync();

            return TypedResults.Ok(new ListLessonSectionsResponse(sections, totalCount));
        }

        /// <summary>
        /// Gets a specific lesson section.
        /// </summary>
        public static async Task<Results<Ok<LessonSectionResponse>, BadRequest<ErrorResponse>>> GetLessonSection(
            string tenant,
            long courseId,
            long lessonId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
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

            long? dbCourseId = lesson.CourseModule?.CourseId;

            bool hasAccess = isInstructor ||
                (userId.HasValue && dbCourseId.HasValue &&
                 await db.CourseEnrollments.AnyAsync(e => e.CourseId == dbCourseId && e.UserProfileId == userId.Value));

            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this lesson section.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            var section = await db.LessonSections
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.LessonId == lessonId);

            if (section == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson section with Id {id} not found in lesson {lessonId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(new LessonSectionResponse(section.Id, section.LessonId, section.Description, section.Order));
        }

        /// <summary>
        /// Creates a new lesson section.
        /// </summary>
        public static async Task<Results<Ok<LessonSectionResponse>, BadRequest<ErrorResponse>>> CreateLessonSection(
            string tenant,
            long courseId,
            long lessonId,
            CreateLessonSectionRequest request,
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
                    "You are not allowed to create lesson sections.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Validate lesson and course
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

            var section = new Shared.Data.Models.Lessons.LessonSection
            {
                LessonId = lessonId,
                Description = request.Description,
                Order = request.Order,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.LessonSections.Add(section);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new LessonSectionResponse(section.Id, section.LessonId, section.Description, section.Order));
        }

        /// <summary>
        /// Updates an existing lesson section.
        /// </summary>
        public static async Task<Results<Ok<LessonSectionResponse>, BadRequest<ErrorResponse>>> UpdateLessonSection(
            string tenant,
            long courseId,
            long lessonId,
            long id,
            UpdateLessonSectionRequest request,
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
                    "You are not allowed to update lesson sections.",
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

            // Validate lesson and course
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

            var section = await db.LessonSections.FirstOrDefaultAsync(s => s.Id == id && s.LessonId == lessonId);
            if (section == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson section with Id {id} not found in lesson {lessonId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            section.Description = request.Description;
            section.Order = request.Order;
            section.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            section.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new LessonSectionResponse(section.Id, section.LessonId, section.Description, section.Order));
        }

        /// <summary>
        /// Soft-deletes a lesson section by marking it as deleted.
        /// </summary>
        public static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteLessonSection(
            string tenant,
            long courseId,
            long lessonId,
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
                    "You are not allowed to delete lesson sections.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Validate lesson and course
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

            var section = await db.LessonSections.FirstOrDefaultAsync(s => s.Id == id && s.LessonId == lessonId);
            if (section == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson section with Id {id} not found in lesson {lessonId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            section.IsDeleted = true;
            section.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            section.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}