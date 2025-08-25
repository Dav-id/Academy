using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionAnswerContracts;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Provides API endpoints for managing assessment question answers.
    /// </summary>
    public static class AssessmentQuestionAnswerEndpoints
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
                .Validate<RouteHandlerBuilder, CreateAssessmentQuestionAnswerRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers");

            app.MapPut("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/answers/{id}", UpdateAnswer)
                .Validate<RouteHandlerBuilder, UpdateAssessmentQuestionAnswerRequest>()
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
        private static async Task<Results<Ok<ListAssessmentQuestionAnswersResponse>, BadRequest<ErrorResponse>>> GetAnswers(
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

            IQueryable<Shared.Data.Models.Assessments.AssessmentQuestionAnswer> query = db.AssessmentQuestionAnswers
                .AsNoTracking()
                .Where(a => a.AssessmentId == assessmentId && a.AssessmentQuestionId == questionId)
                .Include(a => a.SelectedOptions)
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

            List<AssessmentQuestionAnswerResponse> answers = await query
                .OrderBy(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssessmentQuestionAnswerResponse(
                    a.Id,
                    a.AssessmentId,
                    a.AssessmentQuestionId,
                    a.SelectedOptions.Select(o =>
                        new AssessmentQuestionAnswerOptionResponse(
                            o.Id,
                            o.AssessmentQuestionAnswerId,
                            o.AssessmentQuestionOptionId
                        )).ToList()
                ))
                .ToListAsync();

            return TypedResults.Ok(new ListAssessmentQuestionAnswersResponse(answers, totalCount));
        }

        /// <summary>
        /// Gets a specific answer for a question in an assessment.
        /// Only instructors can view any answer; users can only view their own answer.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionAnswerResponse>, BadRequest<ErrorResponse>>> GetAnswer(
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

            IQueryable<Shared.Data.Models.Assessments.AssessmentQuestionAnswer> query = db.AssessmentQuestionAnswers
                .AsNoTracking()
                .Where(a => a.Id == id && a.AssessmentId == assessmentId && a.AssessmentQuestionId == questionId)
                .Include(a => a.SelectedOptions)
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

            AssessmentQuestionAnswerResponse? answer = await query
                .Select(a => new AssessmentQuestionAnswerResponse(
                    a.Id,
                    a.AssessmentId,
                    a.AssessmentQuestionId,
                    a.SelectedOptions.Select(o =>
                        new AssessmentQuestionAnswerOptionResponse(
                            o.Id,
                            o.AssessmentQuestionAnswerId,
                            o.AssessmentQuestionOptionId
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
        private static async Task<Results<Ok<AssessmentQuestionAnswerResponse>, BadRequest<ErrorResponse>>> CreateAnswer(
            string tenant,
            long assessmentId,
            long questionId,
            CreateAssessmentQuestionAnswerRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            long? userId = user?.GetUserId();

            Shared.Data.Models.Assessments.AssessmentQuestionAnswer answer = new()
            {
                AssessmentId = assessmentId,
                AssessmentQuestionId = questionId,
                UserProfileId = userId ?? 0,
                SelectedOptions = [.. request.SelectedOptionIds.Select(optionId =>
                    new Shared.Data.Models.Assessments.AssessmentQuestionAnswerOption
                    {
                        AssessmentQuestionOptionId = optionId
                    })],
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.AssessmentQuestionAnswers.Add(answer);
            await db.SaveChangesAsync();

            AssessmentQuestionAnswerResponse response = new(
                answer.Id,
                answer.AssessmentId,
                answer.AssessmentQuestionId,
                [.. answer.SelectedOptions.Select(o =>
                    new AssessmentQuestionAnswerOptionResponse(
                        o.Id,
                        o.AssessmentQuestionAnswerId,
                        o.AssessmentQuestionOptionId
                    ))]
            );

            return TypedResults.Ok(response);
        }

        /// <summary>
        /// Updates an existing answer for a question in an assessment, including selected options.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionAnswerResponse>, BadRequest<ErrorResponse>>> UpdateAnswer(
            string tenant,
            long assessmentId,
            long questionId,
            long id,
            UpdateAssessmentQuestionAnswerRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            if (id != request.Id || assessmentId != request.AssessmentId || questionId != request.AssessmentQuestionId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentQuestionAnswer? answer = await db.AssessmentQuestionAnswers
                .Include(a => a.SelectedOptions)
                .FirstOrDefaultAsync(a => a.Id == id && a.AssessmentId == assessmentId && a.AssessmentQuestionId == questionId);

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
            db.AssessmentQuestionAnswerOptions.RemoveRange(answer.SelectedOptions);
            answer.SelectedOptions = [.. request.SelectedOptionIds.Select(optionId =>
                new Shared.Data.Models.Assessments.AssessmentQuestionAnswerOption
                {
                    AssessmentQuestionOptionId = optionId
                })];

            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            long? userId = user?.GetUserId();

            answer.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            answer.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            AssessmentQuestionAnswerResponse response = new(
                answer.Id,
                answer.AssessmentId,
                answer.AssessmentQuestionId,
                [.. answer.SelectedOptions.Select(o =>
                    new AssessmentQuestionAnswerOptionResponse(
                        o.Id,
                        o.AssessmentQuestionAnswerId,
                        o.AssessmentQuestionOptionId
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
            Shared.Data.Models.Assessments.AssessmentQuestionAnswer? answer = await db.AssessmentQuestionAnswers
                .Include(a => a.SelectedOptions)
                .FirstOrDefaultAsync(a => a.Id == id && a.AssessmentId == assessmentId && a.AssessmentQuestionId == questionId);

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

            db.AssessmentQuestionAnswerOptions.RemoveRange(answer.SelectedOptions);
            db.AssessmentQuestionAnswers.Remove(answer);
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}