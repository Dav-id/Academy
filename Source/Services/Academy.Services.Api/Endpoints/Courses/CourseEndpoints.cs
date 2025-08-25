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
            Routes.Add("GET: /{tenant}/api/v1/courses?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/courses/{id}", GetCourse)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{id}");

            app.MapPost("/{tenant}/api/v1/courses", CreateCourse)
                .Validate<RouteHandlerBuilder, CreateCourseRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses");

            app.MapPost("/{tenant}/api/v1/courses/{id}", UpdateCourse)
                .Validate<RouteHandlerBuilder, UpdateCourseRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{id}");

            app.MapDelete("/{tenant}/api/v1/courses/{id}", DeleteCourse)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{id}");
        }

        /// <summary>
        /// Retrieves a list of all courses for the current tenant that the user has access to.
        /// Instructors see all courses; users see only their enrolled courses.
        /// </summary>
        public static async Task<Results<Ok<ListCoursesResponse>, BadRequest<ErrorResponse>>> GetCourses(
            string tenant,
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

            bool isInstructor = user.IsInRole($"{tenant}:Instructor");
            long? userId = user?.GetUserId();

            IQueryable<Shared.Data.Models.Courses.Course> query;

            if (isInstructor)
            {
                query = db.Courses.AsNoTracking();
            }
            else if (userId.HasValue)
            {
                query = db.Courses
                    .AsNoTracking()
                    .Where(c => c.Enrollments.Any(e => e.UserProfileId == userId.Value));
            }
            else
            {
                return TypedResults.Ok(new ListCoursesResponse([], 0));
            }

            int totalCount = await query.CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<CourseResponse> courses = await query
                .OrderBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CourseResponse(c.Id, c.Title, c.Description))
                .ToListAsync();

            return TypedResults.Ok(new ListCoursesResponse(courses, totalCount));
        }

        /// <summary>
        /// Retrieves a specific course by ID if the user has access.
        /// Instructors can access any course; users only their enrolled courses.
        /// </summary>
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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            bool isInstructor = user.IsInRole($"{tenant}:Instructor");
            long? userId = user.GetUserId();

            IQueryable<Shared.Data.Models.Courses.Course> query = db.Courses
                .AsNoTracking()
                .Where(c => c.Id == id);

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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(course);
        }

        /// <summary>
        /// Creates a new course.
        /// </summary>
        public static async Task<Results<Ok<CourseResponse>, BadRequest<ErrorResponse>>> CreateCourse(
            string tenant,
            CreateCourseRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to create courses.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Courses.Course course = new()
            {
                Title = request.Title,
                Description = request.Description,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.Courses.Add(course);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new CourseResponse(course.Id, course.Title, course.Description));
        }

        /// <summary>
        /// Updates an existing course.
        /// </summary>
        public static async Task<Results<Ok<CourseResponse>, BadRequest<ErrorResponse>>> UpdateCourse(
            string tenant,
            long id,
            UpdateCourseRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to update courses.",
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

            Shared.Data.Models.Courses.Course? course = await db.Courses.FindAsync(id);
            if (course == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Course with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            course.Title = request.Title;
            course.Description = request.Description;
            course.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            course.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new CourseResponse(course.Id, course.Title, course.Description));
        }

        /// <summary>
        /// Soft-deletes a course by marking it as deleted.
        /// </summary>
        public static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteCourse(
            string tenant,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole($"{tenant}:Instructor") ?? false;
            if (!isInstructor)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You are not allowed to delete courses.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            course.IsDeleted = true;
            course.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            course.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}