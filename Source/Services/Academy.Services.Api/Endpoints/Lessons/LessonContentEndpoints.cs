using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Lessons.LessonContentContracts;

namespace Academy.Services.Api.Endpoints.Lessons
{
    /// <summary>
    /// Provides API endpoints for managing lesson content.
    /// </summary>
    public static class LessonContentEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/lessons/{lessonId}/contents", GetLessonContents)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/lessons/{lessonId}/contents?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/lessons/{lessonId}/contents/{id}", GetLessonContent)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/lessons/{lessonId}/contents/{id}");

            app.MapPost("/{tenant}/api/v1/lessons/{lessonId}/contents", CreateLessonContent)
                .Validate<RouteHandlerBuilder, CreateLessonContentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/lessons/{lessonId}/contents");

            // Update uses POST as it expects the full entity in the body, not just the fields to update.
            app.MapPost("/{tenant}/api/v1/lessons/{lessonId}/contents/{id}", UpdateLessonContent)
                .Validate<RouteHandlerBuilder, UpdateLessonContentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/lessons/{lessonId}/contents/{id}");

            app.MapDelete("/{tenant}/api/v1/lessons/{lessonId}/contents/{id}", DeleteLessonContent)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/lessons/{lessonId}/contents/{id}");
        }

        /// <summary>
        /// Gets all content items for a lesson.
        /// </summary>
        public static async Task<Results<Ok<ListLessonContentsResponse>, BadRequest<ErrorResponse>>> GetLessonContents(
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
            long? userId = user.GetUserId();

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
                    "You do not have access to this lesson content.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            int totalCount = await db.LessonContents
                .AsNoTracking()
                .Where(c => c.LessonId == lessonId)
                .CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<LessonContentResponse> contents = await db.LessonContents
                .AsNoTracking()
                .Where(c => c.LessonId == lessonId)
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new LessonContentResponse(c.Id, c.LessonId, c.ContentType, c.ContentData))
                .ToListAsync();

            return TypedResults.Ok(new ListLessonContentsResponse(contents, totalCount));
        }

        /// <summary>
        /// Gets a specific content item for a lesson.
        /// </summary>
        public static async Task<Results<Ok<LessonContentResponse>, BadRequest<ErrorResponse>>> GetLessonContent(
            string tenant,
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
            long? userId = user.GetUserId();

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
                    "You do not have access to this lesson content.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            LessonContentResponse? content = await db.LessonContents
                .AsNoTracking()
                .Where(c => c.LessonId == lessonId && c.Id == id)
                .Select(c => new LessonContentResponse(c.Id, c.LessonId, c.ContentType, c.ContentData))
                .FirstOrDefaultAsync();

            if (content == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson content with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(content);
        }

        /// <summary>
        /// Creates a new content item for a lesson.
        /// </summary>
        public static async Task<Results<Ok<LessonContentResponse>, BadRequest<ErrorResponse>>> CreateLessonContent(
            string tenant,
            long lessonId,
            CreateLessonContentRequest request,
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
                    "You are not allowed to create lesson content.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.LessonContent content = new()
            {
                LessonId = lessonId,
                ContentType = request.ContentType,
                ContentData = request.ContentData,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.LessonContents.Add(content);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new LessonContentResponse(content.Id, content.LessonId, content.ContentType, content.ContentData));
        }

        /// <summary>
        /// Updates an existing content item for a lesson.
        /// </summary>
        public static async Task<Results<Ok<LessonContentResponse>, BadRequest<ErrorResponse>>> UpdateLessonContent(
            string tenant,
            long lessonId,
            long id,
            UpdateLessonContentRequest request,
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
                    "You are not allowed to update lesson content.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            if (id != request.Id || lessonId != request.LessonId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.LessonContent? content = await db.LessonContents.FirstOrDefaultAsync(c => c.Id == id && c.LessonId == lessonId);
            if (content == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson content with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            content.ContentType = request.ContentType;
            content.ContentData = request.ContentData;
            content.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            content.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new LessonContentResponse(content.Id, content.LessonId, content.ContentType, content.ContentData));
        }

        /// <summary>
        /// Soft-deletes a lesson content item by marking it as deleted.
        /// </summary>
        public static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteLessonContent(
            string tenant,
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
                    "You are not allowed to delete lesson content.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Lessons.LessonContent? content = await db.LessonContents.FirstOrDefaultAsync(c => c.Id == id && c.LessonId == lessonId);
            if (content == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson content with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            content.IsDeleted = true;
            content.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            content.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}