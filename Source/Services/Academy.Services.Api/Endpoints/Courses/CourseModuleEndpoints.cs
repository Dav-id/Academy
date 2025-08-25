using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using static Academy.Services.Api.Endpoints.Courses.CourseModuleContracts;

namespace Academy.Services.Api.Endpoints.Courses
{
    /// <summary>
    /// Provides API endpoints for managing course modules.
    /// </summary>
    public static class CourseModuleEndpoints
    {
        public static readonly List<string> Routes = [];

        /// <summary>
        /// Registers course module endpoints.
        /// </summary>
        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/courses/{courseId}/modules", GetModules)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/modules?page={page}&pageSize={pageSize}");

            app.MapGet("/{tenant}/api/v1/courses/{courseId}/modules/{id}", GetModule)
                .RequireAuthorization();
            Routes.Add("GET: /{tenant}/api/v1/courses/{courseId}/modules/{id}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/modules", CreateModule)
                .Validate<RouteHandlerBuilder, CreateModuleRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/modules");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/modules/{id}", UpdateModule)
                .Validate<RouteHandlerBuilder, UpdateModuleRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization();
            Routes.Add("POST: /{tenant}/api/v1/courses/{courseId}/modules/{id}");

            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/modules/{id}", DeleteModule)
                .RequireAuthorization();
            Routes.Add("DELETE: /{tenant}/api/v1/courses/{courseId}/modules/{id}");
        }

        /// <summary>
        /// Gets all modules for a course, if the user has access to the course.
        /// </summary>
        public static async Task<Results<Ok<ListModulesResponse>, BadRequest<ErrorResponse>>> GetModules(
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

            bool isInstructor = user.IsInRole($"{tenant}:Instructor");
            long? userId = user.GetUserId();

            // Only show modules if user is instructor or enrolled
            bool hasAccess = isInstructor || (userId.HasValue && await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value));
            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this course.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            int totalCount = await db.CourseModules
                .AsNoTracking()
                .Where(m => m.CourseId == courseId)
                .CountAsync();

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            List<ModuleResponse> modules = await db.CourseModules
                .AsNoTracking()
                .Where(m => m.CourseId == courseId)
                .OrderBy(m => m.Order)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new ModuleResponse(m.Id, m.CourseId, m.Title, m.Description, m.Order))
                .ToListAsync();

            return TypedResults.Ok(new ListModulesResponse(modules, totalCount));
        }

        /// <summary>
        /// Gets a specific module for a course, if the user has access.
        /// </summary>
        public static async Task<Results<Ok<ModuleResponse>, BadRequest<ErrorResponse>>> GetModule(
            string tenant,
            long courseId,
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

            bool hasAccess = isInstructor || (userId.HasValue && await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value));
            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this course.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            ModuleResponse? module = await db.CourseModules
                .AsNoTracking()
                .Where(m => m.CourseId == courseId && m.Id == id)
                .Select(m => new ModuleResponse(m.Id, m.CourseId, m.Title, m.Description, m.Order))
                .FirstOrDefaultAsync();

            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(module);
        }

        /// <summary>
        /// Creates a new module for a course.
        /// </summary>
        public static async Task<Results<Ok<ModuleResponse>, BadRequest<ErrorResponse>>> CreateModule(
            string tenant,
            long courseId,
            CreateModuleRequest request,
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
                    "You are not allowed to create modules.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Courses.CourseModule module = new()
            {
                CourseId = courseId,
                Title = request.Title,
                Description = request.Description,
                Order = request.Order,
                CreatedBy = user?.Identity?.Name ?? "Unknown",
                UpdatedBy = user?.Identity?.Name ?? "Unknown",
                TenantId = db.TenantId
            };

            db.CourseModules.Add(module);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new ModuleResponse(module.Id, module.CourseId, module.Title, module.Description, module.Order));
        }

        /// <summary>
        /// Updates an existing module for a course.
        /// </summary>
        public static async Task<Results<Ok<ModuleResponse>, BadRequest<ErrorResponse>>> UpdateModule(
            string tenant,
            long courseId,
            long id,
            UpdateModuleRequest request,
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
                    "You are not allowed to update modules.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            if (id != request.Id || courseId != request.CourseId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Courses.CourseModule? module = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == id && m.CourseId == courseId);
            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            module.Title = request.Title;
            module.Description = request.Description;
            module.Order = request.Order;
            module.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            module.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new ModuleResponse(module.Id, module.CourseId, module.Title, module.Description, module.Order));
        }

        /// <summary>
        /// Soft-deletes a module by marking it as deleted.
        /// </summary>
        public static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteModule(
            string tenant,
            long courseId,
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
                    "You are not allowed to delete modules.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Courses.CourseModule? module = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == id && m.CourseId == courseId);
            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            module.IsDeleted = true;
            module.UpdatedBy = user?.Identity?.Name ?? "Unknown";
            module.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}