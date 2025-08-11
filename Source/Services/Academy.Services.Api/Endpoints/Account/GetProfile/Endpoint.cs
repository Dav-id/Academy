using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Http.HttpResults;

using static Academy.Services.Api.Endpoints.Account.GetProfile.Contracts;

namespace Academy.Services.Api.Endpoints.Account.GetProfile
{
    public static class Endpoint
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoint(this IEndpointRouteBuilder app, IServiceProvider services)
        {
            app.MapGet("api/v1/users/{id}", async (string id, IServiceProvider services) =>
            {
                return await GetProfile(id, services);
            });

            // Log mapped routes
            Routes.Add("api/v1/users/{id}");
        }

        private static async Task<Results<Ok<Response>, BadRequest<ErrorResponse>>> GetProfile(string id, IServiceProvider services)
        {
            await using AsyncServiceScope scope = services.CreateAsyncScope();

            ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(typeof(Endpoint).FullName ?? nameof(Endpoint));
            IHttpContextAccessor httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();

            logger.LogInformation("GetProfile called with Id: {Id}", id);

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

            //TODO: query the database
            //db.Users.Find(id);

            return TypedResults.Ok<Response>(new(id, "First Name", "Last Name", true));
        }
    }
}
