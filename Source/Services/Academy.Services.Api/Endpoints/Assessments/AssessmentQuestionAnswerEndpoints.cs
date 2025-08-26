using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Assessments.AssessmentSectionQuestionAnswerContracts;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Provides API endpoints for managing assessment question answers.
    /// </summary>
    public static class AssessmentSectionQuestionAnswerEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers", GetAnswers)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers/{id}", GetAnswer)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers/{id}");

            app.MapPost("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers", CreateAnswer)
                .Validate<RouteHandlerBuilder, CreateAssessmentSectionQuestionAnswerRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers");

            app.MapPut("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers/{id}", UpdateAnswer)
                .Validate<RouteHandlerBuilder, UpdateAssessmentSectionQuestionAnswerRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("PUT: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers/{id}");

            app.MapDelete("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers/{id}", DeleteAnswer)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers/{id}");
        }

        /// <summary>
        /// Gets all answers for a question in an assessment.
        /// Only instructors can view all answers; users can only view their own answers.
        /// </summary>
        private static async Task<Results<Ok<ListAssessmentSectionQuestionAnswersResponse>, BadRequest<ErrorResponse>>> GetAnswers(
            string tenant,
            long assessmentId,
            long questionId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            int page = 1,
            int pageSize = 20)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            long? userId = user?.GetUserId();

            IQueryable<Shared.Data.Models.Assessments.AssessmentSectionQuestionAnswer> query = db.AssessmentSectionQuestionAnswers
                .AsNoTracking()
                .Where(a => a.AssessmentId == assessmentId && a.QuestionId == questionId)
                .Include(a => a.SelectedOptionAnswers)
                .AsQueryable();

            if (!isInstructor)
            {
                if (!userId.HasValue)
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "User is not authenticated.",
                        null,
                        httpContextAccessor?.HttpContext?.TraceIdentifier
                    ));
                }
                query = query.Where(a => a.UserProfileId == userId.Value);
            }

            int totalCount = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<AssessmentSectionQuestionAnswerResponse> answers = await query
                .OrderBy(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssessmentSectionQuestionAnswerResponse(
                    a.Id,
                    a.AssessmentId,
                    a.QuestionId,
                    a.SelectedOptionAnswers.Select(o =>
                        new AssessmentSectionQuestionAnswerOptionResponse(
                            o.Id,
                            o.AnswerId,
                            o.OptionId
                        )).ToList()
                ))
                .ToListAsync();

            return TypedResults.Ok(new ListAssessmentSectionQuestionAnswersResponse(answers, totalCount));
        }

        /// <summary>
        /// Gets a specific answer for a question in an assessment.
        /// Only instructors can view any answer; users can only view their own answer.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionAnswerResponse>, BadRequest<ErrorResponse>>> GetAnswer(
            string tenant,
            long assessmentId,
            long questionId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            long? userId = user?.GetUserId();

            IQueryable<Shared.Data.Models.Assessments.AssessmentSectionQuestionAnswer> query = db.AssessmentSectionQuestionAnswers
                .AsNoTracking()
                .Where(a => a.Id == id && a.AssessmentId == assessmentId && a.QuestionId == questionId)
                .Include(a => a.SelectedOptionAnswers)
                .AsQueryable();

            if (!isInstructor)
            {
                if (!userId.HasValue)
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "User is not authenticated.",
                        null,
                        httpContextAccessor?.HttpContext?.TraceIdentifier
                    ));
                }
                query = query.Where(a => a.UserProfileId == userId.Value);
            }

            AssessmentSectionQuestionAnswerResponse? answer = await query
                .Select(a => new AssessmentSectionQuestionAnswerResponse(
                    a.Id,
                    a.AssessmentId,
                    a.QuestionId,
                    a.SelectedOptionAnswers.Select(o =>
                        new AssessmentSectionQuestionAnswerOptionResponse(
                            o.Id,
                            o.AnswerId,
                            o.OptionId
                        )).ToList()
                ))
                .FirstOrDefaultAsync();

            if (answer == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question answer with Id {id} not found or not accessible.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(answer);
        }

        /// <summary>
        /// Creates a new answer for a question in an assessment, including selected options.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionAnswerResponse>, BadRequest<ErrorResponse>>> CreateAnswer(
            string tenant,
            long assessmentId,
            long questionId,
            CreateAssessmentSectionQuestionAnswerRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            long? userId = user?.GetUserId();

            Shared.Data.Models.Assessments.AssessmentSectionQuestionAnswer answer = new()
            {
                AssessmentId = assessmentId,
                QuestionId = questionId,
                UserProfileId = userId ?? 0,
                SelectedOptionAnswers = [.. request.SelectedOptionIds.Select(optionId =>
                    new Shared.Data.Models.Assessments.AssessmentSectionQuestionAnswerOption
                    {
                        OptionId = optionId
                    })],
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.AssessmentSectionQuestionAnswers.Add(answer);
            await db.SaveChangesAsync();

            AssessmentSectionQuestionAnswerResponse response = new(
                answer.Id,
                answer.AssessmentId,
                answer.QuestionId,
                [.. answer.SelectedOptionAnswers.Select(o =>
                    new AssessmentSectionQuestionAnswerOptionResponse(
                        o.Id,
                        o.AnswerId,
                        o.OptionId
                    ))]
            );

            return TypedResults.Ok(response);
        }

        /// <summary>
        /// Updates an existing answer for a question in an assessment, including selected options.
        /// </summary>
        private static async Task<Results<Ok<AssessmentSectionQuestionAnswerResponse>, BadRequest<ErrorResponse>>> UpdateAnswer(
            string tenant,
            long assessmentId,
            long questionId,
            long id,
            UpdateAssessmentSectionQuestionAnswerRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            if (id != request.Id || assessmentId != request.AssessmentId || questionId != request.AssessmentSectionQuestionId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentSectionQuestionAnswer? answer = await db.AssessmentSectionQuestionAnswers
                .Include(a => a.SelectedOptionAnswers)
                .FirstOrDefaultAsync(a => a.Id == id && a.AssessmentId == assessmentId && a.QuestionId == questionId);

            if (answer == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question answer with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Remove old options and add new ones
            db.AssessmentSectionQuestionAnswerOptions.RemoveRange(answer.SelectedOptionAnswers);
            answer.SelectedOptionAnswers = [.. request.SelectedOptionIds.Select(optionId =>
                new Shared.Data.Models.Assessments.AssessmentSectionQuestionAnswerOption
                {
                    OptionId = optionId
                })];

            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            long? userId = user?.GetUserId();

            answer.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            answer.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            AssessmentSectionQuestionAnswerResponse response = new(
                answer.Id,
                answer.AssessmentId,
                answer.QuestionId,
                [.. answer.SelectedOptionAnswers.Select(o =>
                    new AssessmentSectionQuestionAnswerOptionResponse(
                        o.Id,
                        o.AnswerId,
                        o.OptionId
                    ))]
            );

            return TypedResults.Ok(response);
        }

        /// <summary>
        /// Deletes an assessment question answer by ID.
        /// </summary>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteAnswer(
            string tenant,
            long assessmentId,
            long questionId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Assessments.AssessmentSectionQuestionAnswer? answer = await db.AssessmentSectionQuestionAnswers
                .Include(a => a.SelectedOptionAnswers)
                .FirstOrDefaultAsync(a => a.Id == id && a.AssessmentId == assessmentId && a.QuestionId == questionId);

            if (answer == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question answer with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            db.AssessmentSectionQuestionAnswerOptions.RemoveRange(answer.SelectedOptionAnswers);
            db.AssessmentSectionQuestionAnswers.Remove(answer);
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}