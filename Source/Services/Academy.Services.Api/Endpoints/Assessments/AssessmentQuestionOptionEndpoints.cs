using Academy.Services.Api.Extensions;
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
            app.MapGet("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options", GetOptions)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options");

            app.MapGet("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options/{id}", GetOption)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options/{id}");

            app.MapPost("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options", CreateOption)
                .Validate<RouteHandlerBuilder, CreateAssessmentQuestionOptionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options");

            app.MapPost("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options/{id}", UpdateOption)
                .Validate<RouteHandlerBuilder, UpdateAssessmentQuestionOptionRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options/{id}");

            app.MapDelete("/{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options/{id}", DeleteOption)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/assessments/{assessmentId}/questions/{questionId}/options/{id}");
        }

        /// <summary>
        /// Gets all options for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<ListAssessmentQuestionOptionsResponse>, BadRequest<ErrorResponse>>> GetOptions(
            string tenant,
            long assessmentId,
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
            long? userId = user.GetUserId();

            // Check question belongs to assessment
            Shared.Data.Models.Assessments.AssessmentQuestion? question = await db.AssessmentQuestions
                .Include(q => q.Assessment)
                    .ThenInclude(a => a.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questionId && q.AssessmentId == assessmentId);

            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to assessment {assessmentId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Only allow access if instructor or enrolled in the course
            long? courseId = question.Assessment?.CourseModule?.CourseId;
            if (!isInstructor)
            {
                if (!userId.HasValue || !courseId.HasValue ||
                    !await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value))
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

            List<AssessmentQuestionOptionResponse> options = await db.AssessmentQuestionOptions
                .AsNoTracking()
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
            long assessmentId,
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
            long? userId = user.GetUserId();

            // Check question belongs to assessment
            Shared.Data.Models.Assessments.AssessmentQuestion? question = await db.AssessmentQuestions
                .Include(q => q.Assessment)
                .ThenInclude(a => a.CourseModule)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questionId && q.AssessmentId == assessmentId);

            if (question == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to assessment {assessmentId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Only allow access if instructor or enrolled in the course
            long? courseId = question.Assessment?.CourseModule?.CourseId;
            if (!isInstructor)
            {
                if (!userId.HasValue || !courseId.HasValue ||
                    !await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value))
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

            AssessmentQuestionOptionResponse? option = await db.AssessmentQuestionOptions
                .AsNoTracking()
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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(option);
        }

        /// <summary>
        /// Creates a new option for an assessment question.
        /// </summary>
        private static async Task<Results<Ok<AssessmentQuestionOptionResponse>, BadRequest<ErrorResponse>>> CreateOption(
            string tenant,
            long assessmentId,
            long questionId,
            CreateAssessmentQuestionOptionRequest request,
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

            bool questionExists = await db.AssessmentQuestions
                .AnyAsync(q => q.Id == questionId && q.AssessmentId == assessmentId);

            if (!questionExists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to assessment {assessmentId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.AssessmentQuestionOption option = new()
            {
                AssessmentQuestionId = questionId,
                OptionText = request.OptionText,
                Score = request.Score,
                IsCorrect = request.IsCorrect,
                Order = request.Order,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
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
            long assessmentId,
            long questionId,
            long id,
            UpdateAssessmentQuestionOptionRequest request,
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

            if (id != request.Id || questionId != request.AssessmentQuestionId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            bool questionExists = await db.AssessmentQuestions
                .AnyAsync(q => q.Id == questionId && q.AssessmentId == assessmentId);

            if (!questionExists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to assessment {assessmentId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
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
            long assessmentId,
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

            bool questionExists = await db.AssessmentQuestions
                .AnyAsync(q => q.Id == questionId && q.AssessmentId == assessmentId);

            if (!questionExists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Question with Id {questionId} does not belong to assessment {assessmentId}.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
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