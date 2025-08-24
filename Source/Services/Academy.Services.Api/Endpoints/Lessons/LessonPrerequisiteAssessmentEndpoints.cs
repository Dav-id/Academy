using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

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
            Routes.Add($"GET: /{{tenant}}/api/v1/lessons/{{lessonId}}/prerequisite-assessments");

            app.MapPost("/{tenant}/api/v1/lessons/{lessonId}/prerequisite-assessments", CreateLessonPrerequisiteAssessment)
                .Validate<RouteHandlerBuilder, CreateLessonPrerequisiteAssessmentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/lessons/{{lessonId}}/prerequisite-assessments");

            app.MapDelete("/{tenant}/api/v1/lessons/{lessonId}/prerequisite-assessments/{prerequisiteAssessmentId}", DeleteLessonPrerequisiteAssessment)                
                .RequireAuthorization("Instructor");
            Routes.Add($"DELETE: /{{tenant}}/api/v1/lessons/{{lessonId}}/prerequisite-assessments/{{prerequisiteAssessmentId}}");
        }

        /// <summary>
        /// Gets all prerequisite assessments for a lesson.
        /// </summary>
        private static async Task<Results<Ok<ListLessonPrerequisiteAssessmentsResponse>, BadRequest<ErrorResponse>>> GetLessonPrerequisiteAssessments(
            string tenant,
            long lessonId,
            ApplicationDbContext db)
        {
            List<LessonPrerequisiteAssessmentResponse> prerequisites = await db.LessonPrerequisiteAssessments
                .Where(p => p.LessonId == lessonId)
                .Select(p => new LessonPrerequisiteAssessmentResponse(p.LessonId, p.PrerequisiteAssessmentId ?? 0))
                .ToListAsync();

            return TypedResults.Ok(new ListLessonPrerequisiteAssessmentsResponse(prerequisites));
        }

        /// <summary>
        /// Creates a new prerequisite assessment for a lesson.
        /// </summary>
        private static async Task<Results<Ok<LessonPrerequisiteAssessmentResponse>, BadRequest<ErrorResponse>>> CreateLessonPrerequisiteAssessment(
            string tenant,
            long lessonId,
            CreateLessonPrerequisiteAssessmentRequest request,
            ApplicationDbContext db)
        {
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
                    null
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
            ApplicationDbContext db)
        {
            Shared.Data.Models.Lessons.LessonPrerequisiteAssessment? prerequisite = await db.LessonPrerequisiteAssessments
                .FirstOrDefaultAsync(p => p.PrerequisiteAssessmentId == prerequisiteAssessmentId && p.LessonId == lessonId);
            if (prerequisite == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Prerequisite assessment with Id {prerequisiteAssessmentId} not found for lesson {lessonId}.",
                    null,
                    null
                ));
            }

            db.LessonPrerequisiteAssessments.Remove(prerequisite);
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}