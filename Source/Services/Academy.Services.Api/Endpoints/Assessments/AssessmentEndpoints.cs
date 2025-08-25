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
            Routes.Add("GET: /{tenant}/api/v1/assessments?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/assessments/{id}", GetAssessment)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/assessments/{id}");

            app.MapPost("/{tenant}/api/v1/assessments", CreateAssessment)
                .Validate<RouteHandlerBuilder, CreateAssessmentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/assessments");

            app.MapPut("/{tenant}/api/v1/assessments/{id}", UpdateAssessment)
                .Validate<RouteHandlerBuilder, UpdateAssessmentRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("PUT: /{tenant}/api/v1/assessments/{id}");

            app.MapDelete("/{tenant}/api/v1/assessments/{id}", DeleteAssessment)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/assessments/{id}");
        }

        /// <summary>
        /// Gets all assessments.
        /// </summary>
        private static async Task<Results<Ok<ListAssessmentsResponse>, BadRequest<ErrorResponse>>> GetAssessments(
            string tenant,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            int page = 1,
            int pageSize = 20)
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

            IQueryable<Shared.Data.Models.Assessments.Assessment> query = db.Assessments.AsNoTracking();

            if (!isInstructor)
            {
                if (!userId.HasValue)
                {
                    return TypedResults.Ok(new ListAssessmentsResponse([], 0));
                }

                // Only include assessments where the user is enrolled in the parent course
                query = query
                    .Where(a =>
                        a.CourseModule != null &&
                        a.CourseModule.Course != null &&
                        a.CourseModule.Course.Enrollments.Any(e => e.UserProfile != null && e.UserProfile.Id == userId.Value)
                    );
            }

            int totalCount = await query.CountAsync();

            if (page < 1) { page = 1; }
            if (pageSize < 1) { pageSize = 20; }
            if (pageSize > 100) { pageSize = 100; }

            List<AssessmentResponse> assessments = await query
                .OrderBy(a => a.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AssessmentResponse(
                    a.Id,
                    a.Title ?? string.Empty,
                    a.Description ?? string.Empty,
                    a.AssessmentType.ConvertAssessmentType(),
                    a.CourseModuleId
                ))
                .ToListAsync();

            return TypedResults.Ok(new ListAssessmentsResponse(assessments, totalCount));
        }

        /// <summary>
        /// Gets a specific assessment by ID.
        /// </summary>
        private static async Task<Results<Ok<AssessmentResponse>, BadRequest<ErrorResponse>>> GetAssessment(
            string tenant,
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

            IQueryable<Shared.Data.Models.Assessments.Assessment> query = db.Assessments.AsNoTracking();

            if (!isInstructor)
            {
                if (!userId.HasValue)
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to access this assessment.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
                }

                // Only include assessments where the user is enrolled in the parent course
                query = query
                    .Where(a =>
                        a.CourseModule != null &&
                        a.CourseModule.Course != null &&
                        a.CourseModule.Course.Enrollments.Any(e => e.UserProfile != null && e.UserProfile.Id == userId.Value)
                    );
            }

            AssessmentResponse? assessment = await query
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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
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
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to create assessments.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Assessments.Assessment assessment = new()
            {
                Title = request.Title,
                Description = request.Description,
                AssessmentType = request.AssessmentType.ConvertAssessmentType(),
                CourseModuleId = request.CourseModuleId,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
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
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to update assessments.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            if (id != request.Id)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            assessment.Title = request.Title;
            assessment.Description = request.Description;
            assessment.AssessmentType = request.AssessmentType.ConvertAssessmentType();
            assessment.CourseModuleId = request.CourseModuleId;
            assessment.UpdatedBy = user?.Identity?.Name ?? "Unknown";
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
            System.Security.Claims.ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to delete assessments.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            assessment.IsDeleted = true;
            assessment.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            assessment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}