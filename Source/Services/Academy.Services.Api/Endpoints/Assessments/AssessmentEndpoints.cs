using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Assessments.AssessmentContracts;

namespace Academy.Services.Api.Endpoints.Assessments
{
    /// <summary>
    /// Provides API endpoints for managing assessments.
    /// </summary>
    public static class AssessmentEndpoints
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {            
            app.MapGet("/{tenant}/api/v1/assessments", GetAssessments)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/assessments");

            app.MapGet("/{tenant}/api/v1/assessments/{id}", GetAssessment)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/assessments/{{id}}");

            app.MapPost("/{tenant}/api/v1/assessments", CreateAssessment)
                .Validate<RouteHandlerBuilder, CreateAssessmentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/assessments");

            app.MapPut("/{tenant}/api/v1/assessments/{id}", UpdateAssessment)
                .Validate<RouteHandlerBuilder, UpdateAssessmentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"PUT: /{{tenant}}/api/v1/assessments/{{id}}");

            app.MapDelete("/{tenant}/api/v1/assessments/{id}", DeleteAssessment)
                .RequireAuthorization("Instructor");
            Routes.Add($"DELETE: /{{tenant}}/api/v1/assessments/{{id}}");
        }

        /// <summary>
        /// Gets all assessments.
        /// </summary>
        private static async Task<Results<Ok<ListAssessmentsResponse>, BadRequest<ErrorResponse>>> GetAssessments(
            string tenant,
            ApplicationDbContext db)
        {
            List<AssessmentResponse> assessments = await db.Assessments
                .Select(a => new AssessmentResponse(
                    a.Id,
                    a.Title ?? string.Empty,
                    a.Description ?? string.Empty,
                    a.AssessmentType.ConvertAssessmentType(),
                    a.CourseModuleId
                ))
                .ToListAsync();

            return TypedResults.Ok(new ListAssessmentsResponse(assessments));
        }

        /// <summary>
        /// Gets a specific assessment by ID.
        /// </summary>
        private static async Task<Results<Ok<AssessmentResponse>, BadRequest<ErrorResponse>>> GetAssessment(
            string tenant,
            long id,
            ApplicationDbContext db)
        {
            AssessmentResponse? assessment = await db.Assessments
                .Where(a => a.Id == id)
                .Select(a => new AssessmentResponse(
                    a.Id,
                    a.Title ?? string.Empty,
                    a.Description ?? string.Empty,
                    a.AssessmentType.ConvertAssessmentType(),
                    a.CourseModuleId
                ))
                .FirstOrDefaultAsync();

            if (assessment == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment with Id {id} not found.",
                    null,
                    null
                ));
            }

            return TypedResults.Ok(assessment);
        }

        /// <summary>
        /// Creates a new assessment.
        /// </summary>
        private static async Task<Results<Ok<AssessmentResponse>, BadRequest<ErrorResponse>>> CreateAssessment(
            string tenant,
            CreateAssessmentRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Assessments.Assessment assessment = new()
            {
                Title = request.Title,
                Description = request.Description,
                AssessmentType = request.AssessmentType.ConvertAssessmentType(),
                CourseModuleId = request.CourseModuleId,
                CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.Assessments.Add(assessment);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentResponse(
                assessment.Id,
                assessment.Title ?? string.Empty,
                assessment.Description ?? string.Empty,
                assessment.AssessmentType.ConvertAssessmentType(),
                assessment.CourseModuleId
            ));
        }

        /// <summary>
        /// Updates an existing assessment.
        /// </summary>
        private static async Task<Results<Ok<AssessmentResponse>, BadRequest<ErrorResponse>>> UpdateAssessment(
            string tenant,
            long id,
            UpdateAssessmentRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            if (id != request.Id)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    null
                ));
            }

            Shared.Data.Models.Assessments.Assessment? assessment = await db.Assessments.FindAsync(id);
            if (assessment == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment with Id {id} not found.",
                    null,
                    null
                ));
            }

            assessment.Title = request.Title;
            assessment.Description = request.Description;
            assessment.AssessmentType = request.AssessmentType.ConvertAssessmentType();
            assessment.CourseModuleId = request.CourseModuleId;
            assessment.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            assessment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new AssessmentResponse(
                assessment.Id,
                assessment.Title ?? string.Empty,
                assessment.Description ?? string.Empty,
                assessment.AssessmentType.ConvertAssessmentType(),
                assessment.CourseModuleId
            ));
        }

        /// <summary>
        /// Deletes an assessment by ID.
        /// </summary>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteAssessment(
            string tenant,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Assessments.Assessment? assessment = await db.Assessments.FindAsync(id);
            if (assessment == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Assessment with Id {id} not found.",
                    null,
                    null
                ));
            }

            assessment.IsDeleted = true;
            assessment.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            assessment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}