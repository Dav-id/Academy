using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Assessments.AssessmentSectionQuestionOptionContracts;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Provides API endpoints for managing assessment question options.
    /// </summary>
    public static class AssessmentSectionQuestionOptionEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            // Updated: courseId is now part of the route hierarchy
            app.MapGet("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options", GetOptions)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options");

            app.MapGet("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options/{id}", GetOption)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options/{id}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options", CreateOption)
                .Validate<RouteHandlerBuilder, CreateAssessmentSectionQuestionOptionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options/{id}", UpdateOption)
                .Validate<RouteHandlerBuilder, UpdateAssessmentSectionQuestionOptionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options/{id}");

            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options/{id}", DeleteOption)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{courseId}/assessments/{assessmentId}/sections/{sectionId}/questions/{questionId}/options/{id}");
        }

        /// <summary>
        /// Gets all options for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<ListAssessmentSectionQuestionOptionsResponse>, BadRequest<ErrorResponse>>> GetOptions(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            long questionId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
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

            // Check question belongs to section, assessment, and course
            var question = await db.AssessmentSectionQuestions
                .Include(q => q.Section)
                    .ThenInclude(s => s.Assessment)
                        .ThenInclude(a => a.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(q =>
                    q.Id == questionId &&
                    q.SectionId == sectionId &&
                    q.Section != null &&
                    q.Section.AssessmentId == assessmentId &&
                    q.Section.Assessment != null &&
                    q.Section.Assessment.CourseModule != null &&
                    q.Section.Assessment.CourseModule.CourseId == courseId
                );

            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to section {sectionId}, assessment {assessmentId}, and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Only allow access if instructor or enrolled in the course
            long? dbCourseId = question.Section?.Assessment?.CourseModule?.CourseId;
            if (!isInstructor)
            {
                if (!userId.HasValue || !dbCourseId.HasValue ||
                    !await db.CourseEnrollments.AnyAsync(e => e.CourseId == dbCourseId && e.UserProfileId == userId.Value))
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                        StatusCodes.Status403Forbidden,
                        "Forbidden",
                        "You do not have access to these options.",
                        null,
                        httpContextAccessor?.HttpContext?.TraceIdentifier
                    ));
                }
            }

            List<AssessmentSectionQuestionOptionResponse> options = await db.AssessmentSectionQuestionOptions
                .AsNoTracking()
                .Where(o => o.QuestionId == questionId)
                .OrderBy(o => o.Order)
                .Select(o => new AssessmentSectionQuestionOptionResponse(
                    o.Id,
                    o.QuestionId,
                    o.OptionText,
                    o.Score,
                    o.IsCorrect,
                    o.Order
                ))
                .ToListAsync();

            return TypedResults.Ok(new ListAssessmentSectionQuestionOptionsResponse(options));
        }

        /// <summary>
        /// Gets a specific option for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionOptionResponse>, BadRequest<ErrorResponse>>> GetOption(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            long questionId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
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

            // Check question belongs to section, assessment, and course
            var question = await db.AssessmentSectionQuestions
                .Include(q => q.Section)
                    .ThenInclude(s => s.Assessment)
                        .ThenInclude(a => a.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(q =>
                    q.Id == questionId &&
                    q.SectionId == sectionId &&
                    q.Section != null &&
                    q.Section.AssessmentId == assessmentId &&
                    q.Section.Assessment != null &&
                    q.Section.Assessment.CourseModule != null &&
                    q.Section.Assessment.CourseModule.CourseId == courseId
                );

            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to section {sectionId}, assessment {assessmentId}, and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Only allow access if instructor or enrolled in the course
            long? dbCourseId = question.Section?.Assessment?.CourseModule?.CourseId;
            if (!isInstructor)
            {
                if (!userId.HasValue || !dbCourseId.HasValue ||
                    !await db.CourseEnrollments.AnyAsync(e => e.CourseId == dbCourseId && e.UserProfileId == userId.Value))
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                        StatusCodes.Status403Forbidden,
                        "Forbidden",
                        "You do not have access to this option.",
                        null,
                        httpContextAccessor?.HttpContext?.TraceIdentifier
                    ));
                }
            }

            AssessmentSectionQuestionOptionResponse? option = await db.AssessmentSectionQuestionOptions
                .AsNoTracking()
                .Where(o => o.QuestionId == questionId && o.Id == id)
                .Select(o => new AssessmentSectionQuestionOptionResponse(
                    o.Id,
                    o.QuestionId,
                    o.OptionText,
                    o.Score,
                    o.IsCorrect,
                    o.Order
                ))
                .FirstOrDefaultAsync();

            if (option == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question option with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(option);
        }

        /// <summary>
        /// Creates a new option for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionOptionResponse>, BadRequest<ErrorResponse>>> CreateOption(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            long questionId,
            CreateAssessmentSectionQuestionOptionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to create assessment question options.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Check question belongs to section, assessment, and course
            bool questionExists = await db.AssessmentSectionQuestions
                .Include(q => q.Section)
                    .ThenInclude(s => s.Assessment)
                        .ThenInclude(a => a.CourseModule)
                .AnyAsync(q =>
                    q.Id == questionId &&
                    q.SectionId == sectionId &&
                    q.Section != null &&
                    q.Section.AssessmentId == assessmentId &&
                    q.Section.Assessment != null &&
                    q.Section.Assessment.CourseModule != null &&
                    q.Section.Assessment.CourseModule.CourseId == courseId
                );

            if (!questionExists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to section {sectionId}, assessment {assessmentId}, and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentSectionQuestionOption option = new()
            {
                QuestionId = questionId,
                OptionText = request.OptionText,
                Score = request.Score,
                IsCorrect = request.IsCorrect,
                Order = request.Order,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.AssessmentSectionQuestionOptions.Add(option);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentSectionQuestionOptionResponse(
                option.Id,
                option.QuestionId,
                option.OptionText,
                option.Score,
                option.IsCorrect,
                option.Order
            ));
        }

        /// <summary>
        /// Updates an existing option for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionOptionResponse>, BadRequest<ErrorResponse>>> UpdateOption(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            long questionId,
            long id,
            UpdateAssessmentSectionQuestionOptionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to update assessment question options.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            if (id != request.Id || questionId != request.AssessmentSectionQuestionId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Check question belongs to section, assessment, and course
            bool questionExists = await db.AssessmentSectionQuestions
                .Include(q => q.Section)
                    .ThenInclude(s => s.Assessment)
                        .ThenInclude(a => a.CourseModule)
                .AnyAsync(q =>
                    q.Id == questionId &&
                    q.SectionId == sectionId &&
                    q.Section != null &&
                    q.Section.AssessmentId == assessmentId &&
                    q.Section.Assessment != null &&
                    q.Section.Assessment.CourseModule != null &&
                    q.Section.Assessment.CourseModule.CourseId == courseId
                );

            if (!questionExists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to section {sectionId}, assessment {assessmentId}, and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentSectionQuestionOption? option = await db.AssessmentSectionQuestionOptions.FirstOrDefaultAsync(o => o.Id == id && o.QuestionId == questionId);
            if (option == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question option with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            option.OptionText = request.OptionText;
            option.Score = request.Score;
            option.IsCorrect = request.IsCorrect;
            option.Order = request.Order;
            option.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            option.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentSectionQuestionOptionResponse(
                option.Id,
                option.QuestionId,
                option.OptionText,
                option.Score,
                option.IsCorrect,
                option.Order
            ));
        }

        /// <summary>
        /// Deletes an assessment question option by ID.
        /// </summary>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteOption(
            string tenant,
            long courseId,
            long assessmentId,
            long sectionId,
            long questionId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to delete assessment question options.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Check question belongs to section, assessment, and course
            bool questionExists = await db.AssessmentSectionQuestions
                .Include(q => q.Section)
                    .ThenInclude(s => s.Assessment)
                        .ThenInclude(a => a.CourseModule)
                .AnyAsync(q =>
                    q.Id == questionId &&
                    q.SectionId == sectionId &&
                    q.Section != null &&
                    q.Section.AssessmentId == assessmentId &&
                    q.Section.Assessment != null &&
                    q.Section.Assessment.CourseModule != null &&
                    q.Section.Assessment.CourseModule.CourseId == courseId
                );

            if (!questionExists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to section {sectionId}, assessment {assessmentId}, and course {courseId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentSectionQuestionOption? option = await db.AssessmentSectionQuestionOptions.FirstOrDefaultAsync(o => o.Id == id && o.QuestionId == questionId);
            if (option == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question option with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            option.IsDeleted = true;
            option.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            option.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}
