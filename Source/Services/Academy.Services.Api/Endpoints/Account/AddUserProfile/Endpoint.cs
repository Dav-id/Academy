using Academy.Shared.Data.Contexts;
using Academy.Shared.Security;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

using static Academy.Services.Api.Endpoints.Account.AddUserProfile.Contracts;

namespace Academy.Services.Api.Endpoints.Account.AddUserProfile
{
    public static class Endpoint
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("api/v1/users", async (Request request, IServiceProvider services) =>
            {
                return await AddUserProfile(request, services);
            })
            .RequireAuthorization("Instructor");

            // Log mapped routes
            Routes.Add("PUT: api/v1/users");
        }

        private static async Task<Results<Ok<Response>, BadRequest<ErrorResponse>>> AddUserProfile(Request request, IServiceProvider services)
        {
            await using AsyncServiceScope scope = services.CreateAsyncScope();

            ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(typeof(Endpoint).FullName ?? nameof(Endpoint));
            IHttpContextAccessor httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

            if (request is null)
            {
                logger.LogError("AddUserProfile called with null request");
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status400BadRequest,
                        "Invalid Request",
                        "The request cannot be null.",
                        "Ensure that the request body is provided.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

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

            Shared.Data.Models.Accounts.UserProfile? up = await db.UserProfiles.FirstOrDefaultAsync(x => x.Id == idpUser.Id);
            if (up == null)
            {
                up = new Shared.Data.Models.Accounts.UserProfile()
                {
                    Id = idpUser.Id,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    Email = idpUser.Email,
                    IsEnabled = idpUser.IsEnabled,
                    CreatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown",
                    UpdatedBy = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Unknown"
                };

                //validate the model
                ValidationContext validationContext = new(up);
                List<ValidationResult> validationResults = [];

                if (!Validator.TryValidateObject(up, validationContext, validationResults, validateAllProperties: true))
                {
                    logger.LogError("AddUserProfile validation failed for user: {Email}", request.Email);

                    return TypedResults.BadRequest(
                        new ErrorResponse(
                            StatusCodes.Status400BadRequest,
                            "Validation Error",
                            "User profile validation failed",
                            string.Join(", ", validationResults.Select(vr => vr.ErrorMessage)),
                            httpContextAccessor.HttpContext?.TraceIdentifier
                        )
                    );
                }

                db.UserProfiles.Add(up);

                await db.SaveChangesAsync();
            }
            else if (!up.IsEnabled)
            {
                up.IsEnabled = true;
                await db.SaveChangesAsync();
            }

            return TypedResults.Ok<Response>(new(up.Id, up.FirstName, up.LastName, up.Email, up.IsEnabled));
        }
    }
}
