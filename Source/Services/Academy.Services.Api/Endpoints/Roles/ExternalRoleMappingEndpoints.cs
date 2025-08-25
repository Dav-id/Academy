using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Roles.ExternalRoleMappingContracts;

namespace Academy.Services.Api.Endpoints.Roles
{
    /// <summary>
    /// Provides API endpoints for managing external role mappings.
    /// </summary>
    public static class ExternalRoleMappingEndpoints
    {
        public static readonly List<string> Routes = [];

        /// <summary>
        /// Registers external role mapping endpoints for CRUD operations.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/{tenant}/api/v1/roles/external-mappings", GetMappings)
                .RequireAuthorization("Administrator");
            Routes.Add("GET: /{tenant}/api/v1/roles/external-mappings");

            app.MapGet("/{tenant}/api/v1/roles/external-mappings/{id}", GetMapping)
                .RequireAuthorization("Administrator");
            Routes.Add("GET: /{tenant}/api/v1/roles/external-mappings/{id}");

            app.MapPost("/{tenant}/api/v1/roles/external-mappings", CreateMapping)
                .Validate<RouteHandlerBuilder, CreateExternalRoleMappingRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Administrator");
            Routes.Add("POST: /{tenant}/api/v1/roles/external-mappings");

            app.MapPut("/{tenant}/api/v1/roles/external-mappings/{id}", UpdateMapping)
                .Validate<RouteHandlerBuilder, UpdateExternalRoleMappingRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Administrator");
            Routes.Add("PUT: /{tenant}/api/v1/roles/external-mappings/{id}");

            app.MapDelete("/{tenant}/api/v1/roles/external-mappings/{id}", DeleteMapping)
                .RequireAuthorization("Administrator");
            Routes.Add("DELETE: /{tenant}/api/v1/roles/external-mappings/{id}");
        }

        /// <summary>
        /// Retrieves all external role mappings for the current tenant.
        /// </summary>
        /// <param name="tenant">The tenant identifier.</param>
        /// <param name="db">The application database context.</param>
        /// <returns>A list of external role mappings.</returns>
        private static async Task<Results<Ok<ListExternalRoleMappingsResponse>, BadRequest<ErrorResponse>>> GetMappings(
            string tenant,
            ApplicationDbContext db)
        {
            List<ExternalRoleMappingResponse> mappings = await db.ExternalRoleMappings
                .Select(m => new ExternalRoleMappingResponse(
                    m.Id, m.Issuer, m.ExternalClaimType, m.ExternalClaimValue, m.AppRole))
                .ToListAsync();

            return TypedResults.Ok(new ListExternalRoleMappingsResponse(mappings));
        }

        /// <summary>
        /// Retrieves a specific external role mapping by ID.
        /// </summary>
        /// <param name="tenant">The tenant identifier.</param>
        /// <param name="id">The external role mapping ID.</param>
        /// <param name="db">The application database context.</param>
        /// <returns>The external role mapping if found; otherwise, a 404 error.</returns>
        private static async Task<Results<Ok<ExternalRoleMappingResponse>, BadRequest<ErrorResponse>>> GetMapping(
            string tenant,
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            ExternalRoleMappingResponse? mapping = await db.ExternalRoleMappings
                .Where(m => m.Id == id)
                .Select(m => new ExternalRoleMappingResponse(
                    m.Id, m.Issuer, m.ExternalClaimType, m.ExternalClaimValue, m.AppRole))
                .FirstOrDefaultAsync();

            if (mapping == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"External role mapping with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            return TypedResults.Ok(mapping);
        }

        /// <summary>
        /// Creates a new external role mapping.
        /// </summary>
        /// <param name="request">The external role mapping creation request.</param>
        /// <param name="db">The application database context.</param>
        /// <returns>The created external role mapping.</returns>
        private static async Task<Results<Ok<ExternalRoleMappingResponse>, BadRequest<ErrorResponse>>> CreateMapping(
            CreateExternalRoleMappingRequest request,
            ApplicationDbContext db)
        {
            Shared.Data.Models.Roles.ExternalRoleMapping mapping = new()
            {
                Issuer = request.Issuer,
                ExternalClaimType = request.ExternalClaimType,
                ExternalClaimValue = request.ExternalClaimValue,
                AppRole = request.AppRole,
                CreatedBy = "System",
                UpdatedBy = "System"
            };

            db.ExternalRoleMappings.Add(mapping);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new ExternalRoleMappingResponse(
                mapping.Id, mapping.Issuer, mapping.ExternalClaimType, mapping.ExternalClaimValue, mapping.AppRole));
        }

        /// <summary>
        /// Updates an existing external role mapping.
        /// </summary>
        /// <param name="id">The external role mapping ID (from route).</param>
        /// <param name="request">The update request.</param>
        /// <param name="db">The application database context.</param>
        /// <returns>The updated external role mapping if found; otherwise, a 404 error.</returns>
        private static async Task<Results<Ok<ExternalRoleMappingResponse>, BadRequest<ErrorResponse>>> UpdateMapping(
            long id,
            UpdateExternalRoleMappingRequest request,
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
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            Shared.Data.Models.Roles.ExternalRoleMapping? mapping = await db.ExternalRoleMappings.FindAsync(id);
            if (mapping == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"External role mapping with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            mapping.Issuer = request.Issuer;
            mapping.ExternalClaimType = request.ExternalClaimType;
            mapping.ExternalClaimValue = request.ExternalClaimValue;
            mapping.AppRole = request.AppRole;
            mapping.UpdatedBy = "System";
            mapping.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new ExternalRoleMappingResponse(
                mapping.Id, mapping.Issuer, mapping.ExternalClaimType, mapping.ExternalClaimValue, mapping.AppRole));
        }

        /// <summary>
        /// Deletes an external role mapping by ID.
        /// </summary>
        /// <param name="id">The external role mapping ID.</param>
        /// <param name="db">The application database context.</param>
        /// <returns>Ok if deleted; otherwise, a 404 error.</returns>
        private static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteMapping(
            long id,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Roles.ExternalRoleMapping? mapping = await db.ExternalRoleMappings.FindAsync(id);
            if (mapping == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"External role mapping with Id {id} not found.",
                    null,
                    httpContextAccessor?.HttpContext?.TraceIdentifier
                ));
            }

            mapping.IsDeleted = true;
            mapping.UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown";
            mapping.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}