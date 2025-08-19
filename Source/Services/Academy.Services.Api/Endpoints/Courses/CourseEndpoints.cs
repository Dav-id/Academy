using Academy.Services.Api.Endpoints.Lessons;
using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Courses.CourseContracts;

namespace Academy.Services.Api.Endpoints.Courses
{
    /// <summary>
    /// Provides API endpoints for managing courses.
    /// </summary>
    public static class CourseEndpoints
    {
        public static readonly List<string> Routes = [];

        /// <summary>
        /// Registers course endpoints for CRUD operations.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {            
            app.MapGet("/{tenant}/api/v1/courses", GetCourses)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses");

            app.MapGet("/{tenant}/api/v1/courses/{id}", GetCourse)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{id}");

            app.MapPost("/{tenant}/api/v1/courses", CreateCourse)
                .Validate<RouteHandlerBuilder, CreateCourseRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add("POST: /{tenant}/api/v1/courses");

            app.MapPut("/{tenant}/api/v1/courses/{id}", UpdateCourse)
                .Validate<RouteHandlerBuilder, UpdateCourseRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add("PUT: /{tenant}/api/v1/courses/{id}");

            app.MapDelete("/{tenant}/api/v1/courses/{id}", DeleteCourse)
                .RequireAuthorization("Instructor");
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{id}");
        }

        /// <summary>
        /// Retrieves a list of all courses for the current tenant that the user has access to.
        /// Instructors see all courses; users see only their enrolled courses.
        /// </summary>
        /// <param name="tenant">The tenant identifier.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>A list of accessible courses.</returns>
        public static async Task<Results<Ok<ListCoursesResponse>, BadRequest<ErrorResponse>>> GetCourses(
            string tenant,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    null
                ));
            }

            bool isInstructor = user.IsInRole("Instructor");
            long? userId = user?.GetUserId();

            IQueryable<Shared.Data.Models.Courses.Course> query;

            if (isInstructor)
            {
                query = db.Courses;
            }
            else if (userId.HasValue)
            {
                query = db.Courses
                    .Where(c => c.Enrollments.Any(e => e.UserProfileId == userId.Value));
            }
            else
            {
                // No user id, return empty
                return TypedResults.Ok(new ListCoursesResponse(new List<CourseResponse>()));
            }

            List<CourseResponse> courses = await query
                .Select(c => new CourseResponse(c.Id, c.Title, c.Description))
                .ToListAsync();

            return TypedResults.Ok(new ListCoursesResponse(courses));
        }

        /// <summary>
        /// Retrieves a specific course by ID if the user has access.
        /// Instructors can access any course; users only their enrolled courses.
        /// </summary>
        /// <param name="tenant">The tenant identifier.</param>
        /// <param name="id">The course ID.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>The course if found and accessible; otherwise, a 404 error.</returns>
        public static async Task<Results<Ok<CourseResponse>, BadRequest<ErrorResponse>>> GetCourse(
            string tenant,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    null
                ));
            }

            bool isInstructor = user.IsInRole("Instructor");
            long? userId = user.GetUserId();

            IQueryable<Shared.Data.Models.Courses.Course> query = db.Courses.Where(c => c.Id == id);

            if (!isInstructor)
            {
                if (!userId.HasValue)
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "User is not authenticated.",
                        null,
                        null
                    ));
                }
                query = query.Where(c => c.Enrollments.Any(e => e.UserProfileId == userId.Value));
            }

            CourseResponse? course = await query
                .Select(c => new CourseResponse(c.Id, c.Title, c.Description))
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Course with Id {id} not found or not accessible.",
                    null,
                    null
                ));
            }

            return TypedResults.Ok(course);
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        /// <param name="request">The course creation request.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>The created course.</returns>
        public static async Task<Results<Ok<CourseResponse>, BadRequest<ErrorResponse>>> CreateCourse(
            CreateCourseRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Courses.Course course = new()
            {
                Title = request.Title,
                Description = request.Description,
                CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new CourseResponse(course.Id, course.Title, course.Description));
        }

        /// <summary>
        /// Updates an existing course.
        /// </summary>
        /// <param name="id">The course ID (from route).</param>
        /// <param name="request">The update request.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>The updated course if found; otherwise, a 404 error.</returns>
        public static async Task<Results<Ok<CourseResponse>, BadRequest<ErrorResponse>>> UpdateCourse(
            long id,
            UpdateCourseRequest request,
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

            Shared.Data.Models.Courses.Course? course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Course with Id {id} not found.",
                    null,
                    null
                ));
            }

            course.Title = request.Title;
            course.Description = request.Description;
            course.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            course.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new CourseResponse(course.Id, course.Title, course.Description));
        }

        /// <summary>
        /// Soft-deletes a course by marking it as deleted.
        /// </summary>
        /// <param name="id">The course ID.</param>
        /// <param name="db">The application database context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        /// <returns>Ok if deleted; otherwise, a 404 error.</returns>
        public static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteCourse(
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Courses.Course? course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Course with Id {id} not found.",
                    null,
                    null
                ));
            }

            course.IsDeleted = true;
            course.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            course.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}