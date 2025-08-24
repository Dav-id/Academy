using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
            Routes.Add($"GET: /{{tenant}}/api/v1/lessons/{{lessonId}}/contents");

            app.MapGet("/{tenant}/api/v1/lessons/{lessonId}/contents/{id}", GetLessonContent)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/lessons/{{lessonId}}/contents/{{id}}");

            app.MapPost("/{tenant}/api/v1/lessons/{lessonId}/contents", CreateLessonContent)
                .Validate<RouteHandlerBuilder, CreateLessonContentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/lessons/{{lessonId}}/contents");

            // Update uses POST as it expects the full entity in the body, not just the fields to update.
            app.MapPost("/{tenant}/api/v1/lessons/{lessonId}/contents/{id}", UpdateLessonContent)
                .Validate<RouteHandlerBuilder, UpdateLessonContentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/lessons/{{lessonId}}/contents/{{id}}");

            app.MapDelete("/{tenant}/api/v1/lessons/{lessonId}/contents/{id}", DeleteLessonContent)
                .RequireAuthorization("Instructor");
            Routes.Add($"DELETE: /{{tenant}}/api/v1/lessons/{{lessonId}}/contents/{{id}}");
        }

        /// <summary>
        /// Gets all content items for a lesson.
        /// </summary>
        public static async Task<Results<Ok<ListLessonContentsResponse>, BadRequest<ErrorResponse>>> GetLessonContents(
            string tenant,
            long lessonId,
            ApplicationDbContext db)
        {
            List<LessonContentResponse> contents = await db.LessonContents
                .Where(c => c.LessonId == lessonId)
                .Select(c => new LessonContentResponse(c.Id, c.LessonId, c.ContentType, c.ContentData))
                .ToListAsync();

            return TypedResults.Ok(new ListLessonContentsResponse(contents));
        }

        /// <summary>
        /// Gets a specific content item for a lesson.
        /// </summary>
        public static async Task<Results<Ok<LessonContentResponse>, BadRequest<ErrorResponse>>> GetLessonContent(
            string tenant,
            long lessonId,
            long id,
            ApplicationDbContext db)
        {
            LessonContentResponse? content = await db.LessonContents
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
                    null
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
            Shared.Data.Models.Lessons.LessonContent content = new()
            {
                LessonId = lessonId,
                ContentType = request.ContentType,
                ContentData = request.ContentData,
                CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
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
            if (id != request.Id || lessonId != request.LessonId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    null
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
                    null
                ));
            }

            content.ContentType = request.ContentType;
            content.ContentData = request.ContentData;
            content.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
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
            Shared.Data.Models.Lessons.LessonContent? content = await db.LessonContents.FirstOrDefaultAsync(c => c.Id == id && c.LessonId == lessonId);
            if (content == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Lesson content with Id {id} not found.",
                    null,
                    null
                ));
            }

            content.IsDeleted = true;
            content.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            content.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}