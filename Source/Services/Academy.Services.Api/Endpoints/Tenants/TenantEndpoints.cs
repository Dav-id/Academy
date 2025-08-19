using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Tenants.TenantContracts;

namespace Academy.Services.Api.Endpoints.Tenants
{
    /// <summary>
    /// Provides API endpoints for managing tenants.
    /// </summary>
    public static class TenantEndpoints
    {
        public static readonly List<string> Routes = [];

        /// <summary>
        /// Registers tenant endpoints for CRUD operations.
        /// </summary>
        /// <param name="app">The endpoint route builder.</param>
        public static void AddEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/tenants", GetTenants)
                .RequireAuthorization("Administrator");
            Routes.Add("GET: /api/v1/tenants");

            app.MapGet("/api/v1/tenants/{tenant}", GetTenant)
                .RequireAuthorization("Administrator");
            Routes.Add("GET: /api/v1/tenants/{tenant}");

            app.MapPost("/api/v1/tenants", CreateTenant)
                .Validate<RouteHandlerBuilder, CreateTenantRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Administrator");
            Routes.Add("POST: /api/v1/tenants");

            app.MapPut("/api/v1/tenants/{tenant}", UpdateTenant)
                .Validate<RouteHandlerBuilder, UpdateTenantRequest>()
                .ProducesValidationProblem()
                .RequireAuthorization("Administrator");
            Routes.Add("PUT: /api/v1/tenants/{tenant}");

            app.MapDelete("/api/v1/tenants/{tenant}", DeleteTenant)
                .RequireAuthorization("Administrator");
            Routes.Add("DELETE: /api/v1/tenants/{tenant}");
        }

        /// <summary>
        /// Retrieves a list of all tenants.
        /// </summary>
        public static async Task<Results<Ok<ListTenantsResponse>, BadRequest<ErrorResponse>>> GetTenants(
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            List<TenantResponse> tenants = await db.Tenants
                .Where(t => !t.IsDeleted)
                .Select(t => new TenantResponse(t.Id, t.UrlStub, t.Title, t.Description))
                .ToListAsync();

            return TypedResults.Ok(new ListTenantsResponse(tenants));
        }

        /// <summary>
        /// Retrieves a specific tenant by ID.
        /// </summary>
        public static async Task<Results<Ok<TenantResponse>, BadRequest<ErrorResponse>>> GetTenant(
            string tenant,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            TenantResponse? t = await db.Tenants
                .Where(t => t.UrlStub == tenant && !t.IsDeleted)
                .Select(t => new TenantResponse(t.Id, t.UrlStub, t.Title, t.Description))
                .FirstOrDefaultAsync();

            if (t == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Tenant with UrlStub {tenant} not found.",
                    null,
                    null
                ));
            }

            return TypedResults.Ok(t);
        }

        /// <summary>
        /// Creates a new tenant.
        /// </summary>
        public static async Task<Results<Ok<TenantResponse>, BadRequest<ErrorResponse>>> CreateTenant(
            CreateTenantRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Tenants.Tenant tenant = new Shared.Data.Models.Tenants.Tenant
            {
                UrlStub = request.UrlStub,
                Title = request.Title,
                Description = request.Description ?? string.Empty,
                IsDeleted = false
            };

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync();

            return TypedResults.Ok(new TenantResponse(tenant.Id, tenant.UrlStub, tenant.Title, tenant.Description));
        }

        /// <summary>
        /// Updates an existing tenant.
        /// </summary>
        public static async Task<Results<Ok<TenantResponse>, BadRequest<ErrorResponse>>> UpdateTenant(
            string tenant,
            UpdateTenantRequest request,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {

            Shared.Data.Models.Tenants.Tenant? t = await db.Tenants.FirstOrDefaultAsync(t => t.UrlStub == tenant);
            if (t == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Tenant with UrlStub {tenant} not found.",
                    null,
                    null
                ));
            }

            t.UrlStub = request.UrlStub;
            t.Title = request.Title;
            t.Description = request.Description ?? string.Empty;

            await db.SaveChangesAsync();

            return TypedResults.Ok(new TenantResponse(t.Id, t.UrlStub, t.Title, t.Description));
        }

        /// <summary>
        /// Soft-deletes a tenant by marking it as deleted.
        /// </summary>
        public static async Task<Results<Ok, BadRequest<ErrorResponse>>> DeleteTenant(
            string tenant,
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor)
        {
            Shared.Data.Models.Tenants.Tenant? t = await db.Tenants.FirstOrDefaultAsync(t => t.UrlStub == tenant);
            if (t == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    $"Tenant with UrlStub {tenant} not found.",
                    null,
                    null
                ));
            }

            t.IsDeleted = true;
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}