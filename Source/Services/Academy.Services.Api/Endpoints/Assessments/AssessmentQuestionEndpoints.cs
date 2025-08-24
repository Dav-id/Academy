using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionContracts;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Provides API endpoints for managing assessment questions.
    /// </summary>
    public static class AssessmentQuestionEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/assessments/{assessmentId}/questions", GetAssessmentQuestions)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/assessments/{{assessmentId}}/questions");

            app.MapGet("/{tenant}/api/v1/assessments/{assessmentId}/questions/{id}", GetAssessmentQuestion)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/assessments/{{assessmentId}}/questions/{{id}}");

            app.MapPost("/{tenant}/api/v1/assessments/{assessmentId}/questions", CreateAssessmentQuestion)
                .Validate<RouteHandlerBuilder, CreateAssessmentQuestionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add($"POST: /{{tenant}}/api/v1/assessments/{{assessmentId}}/questions");

            app.MapPut("/{tenant}/api/v1/assessments/{assessmentId}/questions/{id}", UpdateAssessmentQuestion)
                .Validate<RouteHandlerBuilder, UpdateAssessmentQuestionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization(); 
            Routes.Add($"PUT: /{{tenant}}/api/v1/assessments/{{assessmentId}}/questions/{{id}}");

            app.MapDelete("/{tenant}/api/v1/assessments/{assessmentId}/questions/{id}", DeleteAssessmentQuestion)
                .RequireAuthorization(); 
            Routes.Add($"DELETE: /{{tenant}}/api/v1/assessments/{{assessmentId}}/questions/{{id}}");
        }

        /// <summary>
        /// Gets all questions for an assessment.
        /// </summary>
        private static async Task<Results<Ok<ListAssessmentQuestionsResponse>, BadRequest<ErrorResponse>>> GetAssessmentQuestions(
            string tenant,
            long assessmentId,
            ApplicationDbContext db)
        {
            List<AssessmentQuestionResponse> questions = await db.AssessmentQuestions
                .Where(q => q.AssessmentId == assessmentId)
                .OrderBy(q => q.Order)
                .Select(q => new AssessmentQuestionResponse(
                    q.Id,
                    q.AssessmentId,
                    q.QuestionText,
                    q.QuestionType.ConvertQuizQuestionType(),
                    q.Order,
                    q.MinimumOptionChoiceSelections,
                    q.MaximumOptionChoiceSelections
                ))
                .ToListAsync();

            return TypedResults.Ok(new ListAssessmentQuestionsResponse(questions));
        }

        /// <summary>
        /// Gets a specific question for an assessment.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionResponse>, BadRequest<ErrorResponse>>> GetAssessmentQuestion(
            string tenant,
            long assessmentId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            AssessmentQuestionResponse? question = await db.AssessmentQuestions
                .Where(q => q.AssessmentId == assessmentId && q.Id == id)
                .Select(q => new AssessmentQuestionResponse(
                    q.Id,
                    q.AssessmentId,
                    q.QuestionText,
                    q.QuestionType.ConvertQuizQuestionType(),
                    q.Order,
                    q.MinimumOptionChoiceSelections,
                    q.MaximumOptionChoiceSelections
                ))
                .FirstOrDefaultAsync();

            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(question);
        }

        /// <summary>
        /// Creates a new question for an assessment.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionResponse>, BadRequest<ErrorResponse>>> CreateAssessmentQuestion(
            string tenant,
            long assessmentId,
            CreateAssessmentQuestionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to create assessment questions.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentQuestion question = new()
            {
                AssessmentId = assessmentId,
                QuestionText = request.QuestionText,
                QuestionType = request.QuestionType.ConvertQuizQuestionType(),
                Order = request.Order,
                MinimumOptionChoiceSelections = request.MinimumOptionChoiceSelections,
                MaximumOptionChoiceSelections = request.MaximumOptionChoiceSelections,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.AssessmentQuestions.Add(question);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentQuestionResponse(
                question.Id,
                question.AssessmentId,
                question.QuestionText,
                question.QuestionType.ConvertQuizQuestionType(),
                question.Order,
                question.MinimumOptionChoiceSelections,
                question.MaximumOptionChoiceSelections
            ));
        }

        /// <summary>
        /// Updates an existing question for an assessment.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionResponse>, BadRequest<ErrorResponse>>> UpdateAssessmentQuestion(
            string tenant,
            long assessmentId,
            long id,
            UpdateAssessmentQuestionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to update assessment questions.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            if (id != request.Id || assessmentId != request.AssessmentId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentQuestion? question = await db.AssessmentQuestions.FirstOrDefaultAsync(q => q.Id == id && q.AssessmentId == assessmentId);
            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            question.QuestionText = request.QuestionText;
            question.QuestionType = request.QuestionType.ConvertQuizQuestionType();
            question.Order = request.Order;
            question.MinimumOptionChoiceSelections = request.MinimumOptionChoiceSelections;
            question.MaximumOptionChoiceSelections = request.MaximumOptionChoiceSelections;
            question.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            question.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentQuestionResponse(
                question.Id,
                question.AssessmentId,
                question.QuestionText,
                question.QuestionType.ConvertQuizQuestionType(),
                question.Order,
                question.MinimumOptionChoiceSelections,
                question.MaximumOptionChoiceSelections
            ));
        }

        /// <summary>
        /// Deletes an assessment question by ID.
        /// </summary>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteAssessmentQuestion(
            string tenant,
            long assessmentId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to delete assessment questions.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentQuestion? question = await db.AssessmentQuestions.FirstOrDefaultAsync(q => q.Id == id && q.AssessmentId == assessmentId);
            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            question.IsDeleted = true;
            question.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            question.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}