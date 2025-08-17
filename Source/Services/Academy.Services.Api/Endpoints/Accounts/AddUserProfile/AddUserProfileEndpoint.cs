using Academy.Services.Api.Filters;
using Academy.Shared.Data.Contexts;
using Academy.Shared.Security;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using static Academy.Services.Api.Endpoints.Accounts.AddUserProfile.AddUserProfileContracts;

namespace Academy.Services.Api.Endpoints.Accounts.AddUserProfile
{
    public static class AddUserProfileEndpoint
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("api/v1/users", async (AddUserProfileRequest request, IServiceProvider services) =>
            {
                return await AddUserProfile(request, services);
            })
            .Validate<RouteHandlerBuilder, AddUserProfileRequest>()
            .ProducesValidationProblem()
            .RequireAuthorization("Instructor");

            // Log mapped routes
            Routes.Add("PUT: api/v1/users");
        }

        private static async Task<Results<Ok<AddUserProfileResponse>, BadRequest<ErrorResponse>>> AddUserProfile(AddUserProfileRequest request, IServiceProvider services)
        {
            await using AsyncServiceScope scope = services.CreateAsyncScope();

            ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(typeof(AddUserProfileEndpoint).FullName ?? nameof(AddUserProfileEndpoint));
            IHttpContextAccessor httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            IAuthClient authClient = scope.ServiceProvider.GetRequiredService<IAuthClient>();

            if (authClient == null)
            {
                logger.LogError("AddUserProfile failed to retrieve IAuthClient");

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

            // Check if the user already exists in the Identity Provider
            Shared.Security.Models.UserProfile? idpUser = await authClient.GetUserByEmailAsync(request.Email);
            if (idpUser == null)
            {
                idpUser = await authClient.CreateUserAsync(request.FirstName, request.LastName, request.Email);

                if (idpUser == null)
                {
                    logger.LogError("AddUserProfile failed to create user in Identity Provider");
                    return TypedResults.BadRequest(
                        new ErrorResponse(
                            StatusCodes.Status500InternalServerError,
                            "Internal Server Error",
                            "Failed to create user in Identity Provider",
                            null,
                            httpContextAccessor.HttpContext?.TraceIdentifier
                        )
                    );
                }
            }

            // Ensure the user has an Id
            if (string.IsNullOrEmpty(idpUser.Id))
            {
                logger.LogError("AddUserProfile failed to retrieve user Id from Identity Provider for email: {Email}", request.Email);
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "Failed to retrieve user Id from Identity Provider",
                        null,
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

            // Check if the user profile already exists in the database
            Shared.Data.Models.Accounts.UserProfile? up = await db.UserProfiles.FirstOrDefaultAsync(x => x.IdentityProvider == authClient.ProviderName && x.IdentityProviderId == idpUser.Id);
            if (up == null)
            {
                try
                {
                    up = new Shared.Data.Models.Accounts.UserProfile()
                    {
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        Email = idpUser.Email,

                        IdentityProvider = authClient.ProviderName,
                        IdentityProviderId = idpUser.Id,

                        IsEnabled = idpUser.IsEnabled,
                        CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                        UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown"
                    };

                    // Add the new user profile to the database
                    db.UserProfiles.Add(up);

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "AddUserProfile failed to create user profile for Id: {Id}", idpUser.Id);
                    return TypedResults.BadRequest(
                        new ErrorResponse(
                            StatusCodes.Status500InternalServerError,
                            "Internal Server Error",
                            "Failed to create user profile",
                            null,
                            httpContextAccessor.HttpContext?.TraceIdentifier
                        )
                    );
                }
            }
            // If the user profile already exists, check if it is enabled, if not, enable it
            else if (!up.IsEnabled)
            {
                try
                {
                    up.IsEnabled = true;
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "AddUserProfile failed to enable existing user profile for Id: {Id}", idpUser.Id);
                    return TypedResults.BadRequest(
                        new ErrorResponse(
                            StatusCodes.Status500InternalServerError,
                            "Internal Server Error",
                            "Failed to enable user profile",
                            null,
                            httpContextAccessor.HttpContext?.TraceIdentifier
                        )
                    );
                }
            }

            // Return the user profile response
            return TypedResults.Ok<AddUserProfileResponse>(new(up.Id, up.FirstName, up.LastName, up.Email, up.IsEnabled));
        }
    }
}
