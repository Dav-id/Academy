using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Assessments.AssessmentQuestionOptionContracts;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Provides API endpoints for managing assessment question options.
    /// </summary>
    public static class AssessmentQuestionOptionEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {            
            app.MapGet("/{tenant}/api/v1/questions/{questionId}/options", GetOptions)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/questions/{{questionId}}/options");

            app.MapGet("/{tenant}/api/v1/questions/{questionId}/options/{id}", GetOption)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/questions/{{questionId}}/options/{{id}}");

            app.MapPost("/{tenant}/api/v1/questions/{questionId}/options", CreateOption)
                .Validate<RouteHandlerBuilder, CreateAssessmentQuestionOptionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/questions/{{questionId}}/options");

            app.MapPut("/{tenant}/api/v1/questions/{questionId}/options/{id}", UpdateOption)
                .Validate<RouteHandlerBuilder, UpdateAssessmentQuestionOptionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"PUT: /{{tenant}}/api/v1/questions/{{questionId}}/options/{{id}}");

            app.MapDelete("/{tenant}/api/v1/questions/{questionId}/options/{id}", DeleteOption)
                .RequireAuthorization("Instructor");
            Routes.Add($"DELETE: /{{tenant}}/api/v1/questions/{{questionId}}/options/{{id}}");
        }

        /// <summary>
        /// Gets all options for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<ListAssessmentQuestionOptionsResponse>, BadRequest<ErrorResponse>>> GetOptions(
            string tenant,
            long questionId,
            ApplicationDbContext db)
        {
            List<AssessmentQuestionOptionResponse> options = await db.AssessmentQuestionOptions
                .Where(o => o.AssessmentQuestionId == questionId)
                .OrderBy(o => o.Order)
                .Select(o => new AssessmentQuestionOptionResponse(
                    o.Id,
                    o.AssessmentQuestionId,
                    o.OptionText,
                    o.Score,
                    o.IsCorrect,
                    o.Order
                ))
                .ToListAsync();

            return TypedResults.Ok(new ListAssessmentQuestionOptionsResponse(options));
        }

        /// <summary>
        /// Gets a specific option for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionOptionResponse>, BadRequest<ErrorResponse>>> GetOption(
            string tenant,
            long questionId,
            long id,
            ApplicationDbContext db)
        {
            AssessmentQuestionOptionResponse? option = await db.AssessmentQuestionOptions
                .Where(o => o.AssessmentQuestionId == questionId && o.Id == id)
                .Select(o => new AssessmentQuestionOptionResponse(
                    o.Id,
                    o.AssessmentQuestionId,
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
                    null
                ));
            }

            return TypedResults.Ok(option);
        }

        /// <summary>
        /// Creates a new option for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionOptionResponse>, BadRequest<ErrorResponse>>> CreateOption(
            string tenant,
            long questionId,
            CreateAssessmentQuestionOptionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Assessments.AssessmentQuestionOption option = new()
            {
                AssessmentQuestionId = questionId,
                OptionText = request.OptionText,
                Score = request.Score,
                IsCorrect = request.IsCorrect,
                Order = request.Order,
                CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.AssessmentQuestionOptions.Add(option);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentQuestionOptionResponse(
                option.Id,
                option.AssessmentQuestionId,
                option.OptionText,
                option.Score,
                option.IsCorrect,
                option.Order
            ));
        }

        /// <summary>
        /// Updates an existing option for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionOptionResponse>, BadRequest<ErrorResponse>>> UpdateOption(
            string tenant,
            long questionId,
            long id,
            UpdateAssessmentQuestionOptionRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            if (id != request.Id || questionId != request.AssessmentQuestionId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    null
                ));
            }

            Shared.Data.Models.Assessments.AssessmentQuestionOption? option = await db.AssessmentQuestionOptions.FirstOrDefaultAsync(o => o.Id == id && o.AssessmentQuestionId == questionId);
            if (option == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question option with Id {id} not found.",
                    null,
                    null
                ));
            }

            option.OptionText = request.OptionText;
            option.Score = request.Score;
            option.IsCorrect = request.IsCorrect;
            option.Order = request.Order;
            option.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            option.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentQuestionOptionResponse(
                option.Id,
                option.AssessmentQuestionId,
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
            long questionId,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Assessments.AssessmentQuestionOption? option = await db.AssessmentQuestionOptions.FirstOrDefaultAsync(o => o.Id == id && o.AssessmentQuestionId == questionId);
            if (option == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment question option with Id {id} not found.",
                    null,
                    null
                ));
            }

            option.IsDeleted = true;
            option.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            option.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}