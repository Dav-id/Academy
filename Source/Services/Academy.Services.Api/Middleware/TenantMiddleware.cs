using Academy.Shared.Data.Contexts;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Academy.Services.Api.Middleware
{
    /// <summary>
    /// Middleware to resolve tenant ID from the tenant url stub in the request and set it in the database context.
    /// </summary>
    public class TenantMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context, ApplicationDbContext db, IDistributedCache cache)
        {
            RouteValueDictionary routeValues = context.Request.RouteValues;

            if (routeValues.TryGetValue("tenant", out object? tenantObj) && tenantObj is string tenantStub)
            {

                string cacheKey = $"tenant:{tenantStub.ToLower()}";
                string tenantIdString = await cache.GetStringAsync(cacheKey) ?? "";
                long tenantId = 0;
                if (string.IsNullOrEmpty(tenantIdString))
                {
                    tenantId = (await db.Tenants.AsNoTracking()
                                                .Where(t => t.UrlStub == tenantStub)
                                                .Select(t => new { t.Id })
                                                .FirstOrDefaultAsync())?.Id ?? 0;
                    
                    // If tenantId is still 0, it means tenant was not found
                    if (tenantId == 0)
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.WriteAsync("Tenant not found.");
                        return;
                    }

                    // Cache for 1 hour (adjust as needed)
                    await cache.SetStringAsync(cacheKey, tenantId.ToString(), new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
                }
                else
                {
                    _ = long.TryParse(tenantIdString, out tenantId);
                }

                if (tenantId == 0)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Tenant not found.");
                    return;
                }

                db.SetTenant(tenantId);
            }

            await next(context);
        }
    }
}
