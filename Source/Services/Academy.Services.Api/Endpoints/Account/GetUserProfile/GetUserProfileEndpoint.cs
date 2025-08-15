using Academy.Services.Api.Extensions;
using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.IdentityModel.Tokens;

using static Academy.Services.Api.Endpoints.Account.GetUserProfile.GetUserProfileContracts;

namespace Academy.Services.Api.Endpoints.Account.GetUserProfile
{
    public static class GetUserProfileEndpoint
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("api/v1/users/{id}", async (string id, IServiceProvider services) =>
            {
                return await GetProfile(id, services);
            });

            // Log mapped routes
            Routes.Add("GET: api/v1/users/{id}");
        }

        private static async Task<Results<Ok<Response>, BadRequest<ErrorResponse>>> GetProfile(string id, IServiceProvider services)
        {
            await using AsyncServiceScope scope = services.CreateAsyncScope();

            ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(typeof(GetUserProfileEndpoint).FullName ?? nameof(GetUserProfileEndpoint));
            IHttpContextAccessor httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

            //logger.LogInformation("GetProfile called with Id: {Id}", id);

            CaseSensitiveClaimsIdentity? cp = httpContextAccessor.HttpContext?.User?.Identity as CaseSensitiveClaimsIdentity;
            if (cp == null)
            {
                logger.LogError("GetProfile called without an authenticated user");
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        "User is not authenticated.",
                        "Ensure that the user is authenticated before making this request.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }


            if (!(cp?.IsInRole("Administrator") ?? false) && !(cp?.IsInRole("Instructor") ?? false) && cp?.GetUserId() != id)
            {
                logger.LogError("GetProfile called by user without sufficient permissions. UserId: {UserId}, RequestedId: {RequestedId}", cp.GetUserId(), id);
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status403Forbidden,
                        "Forbidden",
                        "You do not have permission to access this resource.",
                        "Ensure that you have the necessary permissions to access this user profile.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                logger.LogError("GetProfile called with empty or null Id");

                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status400BadRequest,
                        "Invalid Request",
                        "The Id cannot be null or empty.",
                        "Ensure that the Id is provided in the request body.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            await using ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (db == null)
            {
                logger.LogError("GetProfile failed to retrieve ApplicationDbContext");
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "Internal Server Error",
                        null,
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            var up = db.UserProfiles.FirstOrDefault(x => x.Id == id);
            if (up == null)
            {
                logger.LogError("GetProfile failed to find user profile with Id: {Id}", id);
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status404NotFound,
                        "User Not Found",
                        $"No user profile found with Id: {id}",
                        null,
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            return TypedResults.Ok<Response>(new(up.Id, up.FirstName, up.LastName, up.Email, up.IsEnabled));
        }
    }
}
