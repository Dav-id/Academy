using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Courses.CourseEnrollmentContracts;

namespace Academy.Services.Api.Endpoints.Courses
{
    /// <summary>
    /// Provides API endpoints for managing course enrollments.
    /// </summary>
    public static class CourseEnrollmentEndpoints
    {

        public static readonly List<string> Routes = [];

        /// <summary>
        /// Registers course enrollment endpoints.
        /// </summary>
        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/courses/{courseId}/enrollments", GetCourseEnrollments)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/enrollments?page={page}&pageSize={pageSize}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/enroll", EnrollInCourse)
                .Validate<RouteHandlerBuilder, EnrollRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/enroll");

            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/enroll", UnenrollFromCourse)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{courseId}/enroll");
        }

        /// <summary>
        /// Enrolls the current user in a course.
        /// </summary>
        public static async Task<Results<Ok<EnrollmentResponse>, BadRequest<ErrorResponse>>> EnrollInCourse(
            string tenant,
            long courseId,
            EnrollRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to enroll users in courses.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            long? userId = user?.GetUserId();
            if (userId is null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            // Check if already enrolled
            Shared.Data.Models.Courses.CourseEnrollment? existing = await db.CourseEnrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value);

            if (existing != null)
            {
                return TypedResults.Ok(new EnrollmentResponse(existing.Id, existing.CourseId, existing.UserProfileId, existing.EnrolledOn, existing.IsCompleted));
            }

            Shared.Data.Models.Courses.CourseEnrollment enrollment = new()
            {
                CourseId = courseId,
                UserProfileId = userId.Value,
                EnrolledOn = DateTime.UtcNow,
                IsCompleted = false,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.CourseEnrollments.Add(enrollment);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new EnrollmentResponse(enrollment.Id, enrollment.CourseId, enrollment.UserProfileId, enrollment.EnrolledOn, enrollment.IsCompleted));
        }

        /// <summary>
        /// Unenrolls the current user from a course.
        /// </summary>
        public static async Task<Results<Ok, BadRequest<ErrorResponse>>> UnenrollFromCourse(
            string tenant,
            long courseId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = (user?.IsInRole($"{tenant}:Instructor") ?? false) || (user?.IsInRole($"{tenant}:Administrator") ?? false) || (user?.IsInRole("Administrator") ?? false);
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to unenroll users from courses.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            long? userId = user?.GetUserId();
            if (userId is null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Courses.CourseEnrollment? enrollment = await db.CourseEnrollments
                .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value);

            if (enrollment == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Enrollment for course {courseId} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            enrollment.IsDeleted = true;
            enrollment.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            enrollment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }

        /// <summary>
        /// Gets all enrollments for a course.
        /// </summary>
        public static async Task<Results<Ok<ListEnrollmentsResponse>, BadRequest<ErrorResponse>>> GetCourseEnrollments(
            string tenant,
            long courseId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            int page = 1,
            int pageSize = 20)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
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

            IQueryable<Shared.Data.Models.Courses.CourseEnrollment> query;

            if (isInstructor)
            {
                query = db.CourseEnrollments
                    .AsNoTracking()
                    .Where(e => e.CourseId == courseId);
            }
            else if (userId.HasValue)
            {
                query = db.CourseEnrollments
                    .AsNoTracking()
                    .Where(e => e.CourseId == courseId && e.UserProfileId == userId.Value);
            }
            else
            {
                return TypedResults.Ok(new ListEnrollmentsResponse([], 0));
            }

            int totalCount = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<EnrollmentResponse> enrollments = await query
                .OrderBy(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EnrollmentResponse(e.Id, e.CourseId, e.UserProfileId, e.EnrolledOn, e.IsCompleted))
                .ToListAsync();

            return TypedResults.Ok(new ListEnrollmentsResponse(enrollments, totalCount));
        }
    }
}