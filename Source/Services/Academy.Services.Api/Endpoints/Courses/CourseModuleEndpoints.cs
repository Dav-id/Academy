using Academy.Services.Api.Endpoints.Lessons;
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
            Routes.Add($"GET: /{{tenant}}/api/v1/courses/{{courseId}}/modules");

            app.MapGet("/{tenant}/api/v1/courses/{courseId}/modules/{id}", GetModule)
                .RequireAuthorization();
            Routes.Add($"GET: /{{tenant}}/api/v1/courses/{{courseId}}/modules/{{id}}");

            app.MapPost("/{tenant}/api/v1/courses/{courseId}/modules", CreateModule)
                .Validate<RouteHandlerBuilder, CreateModuleRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"POST: /{{tenant}}/api/v1/courses/{{courseId}}/modules");

            app.MapPut("/{tenant}/api/v1/courses/{courseId}/modules/{id}", UpdateModule)
                .Validate<RouteHandlerBuilder, UpdateModuleRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Instructor");
            Routes.Add($"PUT: /{{tenant}}/api/v1/courses/{{courseId}}/modules/{{id}}");


            app.MapDelete("/{tenant}/api/v1/courses/{courseId}/modules/{id}", DeleteModule)
                .RequireAuthorization("Instructor");
            Routes.Add($"DELETE: /{{tenant}}/api/v1/courses/{{courseId}}/modules/{{id}}");
        }

        /// <summary>
        /// Gets all modules for a course, if the user has access to the course.
        /// </summary>
        public static async Task<Results<Ok<ListModulesResponse>, BadRequest<ErrorResponse>>> GetModules(
            string tenant,
            long courseId,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            bool isInstructor = user?.IsInRole("Instructor") ?? false;
            long? userId = user?.GetUserId();

            // Only show modules if user is instructor or enrolled
            bool hasAccess = isInstructor || (userId.HasValue && await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value));
            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this course.",
                    null,
                    null
                ));
            }

            List<ModuleResponse> modules = await db.CourseModules
                .Where(m => m.CourseId == courseId)
                .OrderBy(m => m.Order)
                .Select(m => new ModuleResponse(m.Id, m.CourseId, m.Title, m.Description, m.Order))
                .ToListAsync();

            return TypedResults.Ok(new ListModulesResponse(modules));
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
            bool isInstructor = user?.IsInRole("Instructor") ?? false;
            long? userId = user?.GetUserId();

            bool hasAccess = isInstructor || (userId.HasValue && await db.CourseEnrollments.AnyAsync(e => e.CourseId == courseId && e.UserProfileId == userId.Value));
            if (!hasAccess)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have access to this course.",
                    null,
                    null
                ));
            }

            ModuleResponse? module = await db.CourseModules
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
                    null
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
            Shared.Data.Models.Courses.CourseModule module = new()
            {
                CourseId = courseId,
                Title = request.Title,
                Description = request.Description,
                Order = request.Order,
                CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
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
            if (id != request.Id || courseId != request.CourseId)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status400BadRequest,
                    "Invalid Request",
                    "Route id and request id do not match.",
                    null,
                    null
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
                    null
                ));
            }

            module.Title = request.Title;
            module.Description = request.Description;
            module.Order = request.Order;
            module.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
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
            Shared.Data.Models.Courses.CourseModule? module = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == id && m.CourseId == courseId);
            if (module == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Module with Id {id} not found.",
                    null,
                    null
                ));
            }

            module.IsDeleted = true;
            module.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            module.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}