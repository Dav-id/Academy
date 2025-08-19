using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
            app.MapGet("/{tenant}/api/v1/lessons/{lessonId}/prerequisites", GetLessonPrerequisites)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/lessons/{{lessonId}}/prerequisites");

            app.MapPost("/{tenant}/api/v1/lessons/{lessonId}/prerequisites", CreateLessonPrerequisite)
                .Validate<RouteHandlerBuilder, CreateLessonPrerequisiteLessonRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/lessons/{{lessonId}}/prerequisites");

            app.MapDelete("/{tenant}/api/v1/lessons/{lessonId}/prerequisites/{prerequisiteLessonId}", DeleteLessonPrerequisite)                
                .RequireAuthorization("Instructor");
            Routes.Add($"DELETE: /{{tenant}}/api/v1/lessons/{{lessonId}}/prerequisites/{{prerequisiteLessonId}}");
        }

        /// <summary>
        /// Gets all prerequisite lessons for a lesson.
        /// </summary>
        private static async Task<Results<Ok<ListLessonPrerequisiteLessonsResponse>, BadRequest<ErrorResponse>>> GetLessonPrerequisites(
            string tenant,
            long lessonId,
            ApplicationDbContext db)
        {
            List<LessonPrerequisiteLessonResponse> prerequisites = await db.LessonPrerequisiteLessons
                .Where(p => p.LessonId == lessonId)
                .Select(p => new LessonPrerequisiteLessonResponse(p.LessonId, p.PrerequisiteLessonId ?? 0))
                .ToListAsync();

            return TypedResults.Ok(new ListLessonPrerequisiteLessonsResponse(prerequisites));
        }

        /// <summary>
        /// Creates a new prerequisite lesson for a lesson.
        /// </summary>
        private static async Task<Results<Ok<LessonPrerequisiteLessonResponse>, BadRequest<ErrorResponse>>> CreateLessonPrerequisite(
            string tenant,
            long lessonId,
            CreateLessonPrerequisiteLessonRequest request,
            ApplicationDbContext db)
        {
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
                    null
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
            long lessonId,
            long prerequisiteLessonId,
            ApplicationDbContext db)
        {
            Shared.Data.Models.Lessons.LessonPrerequisiteLesson? prerequisite = await db.LessonPrerequisiteLessons
                .FirstOrDefaultAsync(p => p.PrerequisiteLessonId == prerequisiteLessonId && p.LessonId == lessonId);
            if (prerequisite == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Prerequisite lesson with Id {prerequisiteLessonId} not found for lesson {lessonId}.",
                    null,
                    null
                ));
            }

            db.LessonPrerequisiteLessons.Remove(prerequisite);
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}