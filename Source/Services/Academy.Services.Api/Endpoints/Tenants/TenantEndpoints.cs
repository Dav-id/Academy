using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;
using Academy.Shared.Data.Models.Accounts;
using Academy.Shared.Security;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

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
                .RequireAuthorization();
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
            ClaimsPrincipal? user = httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "User is not authenticated.",
                    null,
                    httpContextAccessor.HttpContext?.TraceIdentifier
                ));
            }

            bool isAdmin = user.IsInRole("Administrator");
            long? userId = user?.GetUserId();

            IQueryable<Shared.Data.Models.Tenants.Tenant> query;

            if (isAdmin)
            {
                query = db.Tenants.AsNoTracking();
            }
            else if (userId.HasValue)
            {
                query = db.Tenants.AsNoTracking().Where(c => c.Users.Any(e => e.Id == userId.Value));
            }
            else
            {
                // No user id, return empty
                return TypedResults.Ok(new ListTenantsResponse([]));
            }

            List<TenantResponse> tenantResponses = (await query.ToListAsync()).ConvertAll(t => new TenantResponse(t.Id, t.UrlStub, t.Title, t.Description))
;
            return TypedResults.Ok(new ListTenantsResponse(tenantResponses));
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
                    httpContextAccessor.HttpContext?.TraceIdentifier
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
            IHttpContextAccessor httpContextAccessor,
            IAuthClient authClient)
        {
            // Check for duplicate UrlStub
            bool exists = await db.Tenants.AnyAsync(t => t.UrlStub == request.UrlStub && !t.IsDeleted);
            if (exists)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    $"A tenant with UrlStub '{request.UrlStub}' already exists.",
                    null,
                    httpContextAccessor.HttpContext?.TraceIdentifier
                ));
            }

            try
            {
                // 1. Create user via IAuthClient
                Shared.Security.Models.UserProfile? userProfile = await authClient.CreateUserAsync(
                    firstName: request.TenantAccountOwnerFirstName,
                    lastName: request.TenantAccountOwnerLastName,
                    email: request.TenantAccountOwnerEmail                    
                );

                if (userProfile == null)
                {
                    return TypedResults.BadRequest(new ErrorResponse(
                        StatusCodes.Status400BadRequest,
                        "User Creation Failed",
                        "Failed to create the tenant account owner user.",
                        null,
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    ));
                }

                // Create the roles for the tenant
                await authClient.CreateRoleAsync($"{request.UrlStub}:Administrator");
                await authClient.CreateRoleAsync($"{request.UrlStub}:Instructor");
                await authClient.CreateRoleAsync($"{request.UrlStub}:Student");

                // Assign the user to the "Administrator" role
                await authClient.AddUserToRoleAsync(userProfile.Id, $"{request.UrlStub}:Administrator");

                // 2. Create the tenant and associate the user profile
                Shared.Data.Models.Tenants.Tenant tenant = new()
                {
                    UrlStub = request.UrlStub,
                    Title = request.Title,
                    Description = request.Description ?? string.Empty,
                    IsDeleted = false,
                    Users = [ new (){
                        FirstName = request.TenantAccountOwnerFirstName,
                        LastName = request.TenantAccountOwnerLastName,
                        Email = request.TenantAccountOwnerEmail,
                        IdentityProvider = authClient.ProviderName,
                        IdentityProviderId = userProfile.Id,
                        CreatedBy = httpContextAccessor.HttpContext?.User?.GetUserIdString() ?? "Unknown",
                    }]
                };

                db.Tenants.Add(tenant);
                await db.SaveChangesAsync();

                return TypedResults.Ok(new TenantResponse(tenant.Id, tenant.UrlStub, tenant.Title, tenant.Description));
            }
            catch (DbUpdateException dbEx)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Database Error",
                    "An error occurred while saving the tenant.",
                    dbEx.Message,
                    httpContextAccessor.HttpContext?.TraceIdentifier
                ));
            }
            catch (Exception ex)
            {
                return TypedResults.BadRequest(new ErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Unexpected Error",
                    "An unexpected error occurred.",
                    ex.Message,
                    httpContextAccessor.HttpContext?.TraceIdentifier
                ));
            }
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
                    httpContextAccessor.HttpContext?.TraceIdentifier
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
                    httpContextAccessor.HttpContext?.TraceIdentifier
                ));
            }

            t.IsDeleted = true;
            await db.SaveChangesAsync();

            return TypedResults.Ok();
        }
    }
}