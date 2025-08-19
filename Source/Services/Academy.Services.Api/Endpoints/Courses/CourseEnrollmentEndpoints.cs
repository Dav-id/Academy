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
            app.MapPost("/{tenant}/api/v1/courses/{courseId}/enroll", EnrollInCourse)
                .Validate<RouteHandlerBuilder, EnrollRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/courses/{{courseId}}/enroll");

            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/enroll", UnenrollFromCourse)
                .RequireAuthorization("Instructor");
            Routes.Add($"DELETE: /{{tenant}}/api/v1/courses/{{courseId}}/enroll");

            app.MapGet("/{tenant}/api/v1/courses/{courseId}/enrollments", GetCourseEnrollments)
                .RequireAuthorization("Instructor");
            Routes.Add($"GET: /{{tenant}}/api/v1/courses/{{courseId}}/enrollments");
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
            long? userId = user?.GetUserId();
            if (userId is null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    null
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
            long? userId = user?.GetUserId();
            if (userId is null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    null
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
                    null
                ));
            }

            enrollment.IsDeleted = true;
            enrollment.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            enrollment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }

        /// <summary>
        /// Gets all enrollments for a course (instructor only).
        /// </summary>
        public static async Task<Results<Ok<List<EnrollmentResponse>>, BadRequest<ErrorResponse>>> GetCourseEnrollments(
            string tenant,
            long courseId,
            ApplicationDbContext db)
        {
            List<EnrollmentResponse> enrollments = await db.CourseEnrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => new EnrollmentResponse(e.Id, e.CourseId, e.UserProfileId, e.EnrolledOn, e.IsCompleted))
                .ToListAsync();

            return TypedResults.Ok(enrollments);
        }
    }
}