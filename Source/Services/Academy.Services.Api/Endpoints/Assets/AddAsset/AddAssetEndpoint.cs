using Academy.Services.Api.Extensions;
using Academy.Services.Api.Filters;
using Academy.Shared.Storage;
using Academy.Shared.Storage.Models;

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using static Academy.Services.Api.Endpoints.Assets.AddAsset.AddAssetContracts;

namespace Academy.Services.Api.Endpoints.Assets.AddAsset
{
    public static class AddAssetEndpoint
    {
        public static readonly List<string> Routes = [];

        public static void AddEndpoint(this IEndpointRouteBuilder app)
        {
            const string path = "api/v1/assets";

            app.MapPut(path, AddAsset)
            .Accepts<AddAssetRequest>("multipart/form-data")
            .Validate<RouteHandlerBuilder, AddAssetRequest>()
            .DisableAntiforgery()
            .ProducesValidationProblem()
            .RequireAuthorization(); // changed from .RequireAuthorization("Instructor")

            // Log mapped routes
            Routes.Add($"PUT: {path}");
        }

        private static async Task<Results<Ok<AddAssetResponse>, BadRequest<ErrorResponse>>> AddAsset(string tenant, AddAssetRequest request, IServiceProvider services)
        {
            await using AsyncServiceScope scope = services.CreateAsyncScope();

            // Retrieve required services
            ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(typeof(AddAssetEndpoint).FullName ?? nameof(AddAssetEndpoint));
            IHttpContextAccessor httpContextAccessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
            IStorageClient storageClient = scope.ServiceProvider.GetRequiredService<IStorageClient>();

            // Check if the user is authenticated - should be, otherwise this endpoint should not be accessible
            if (httpContextAccessor.HttpContext?.User?.Identity is not CaseSensitiveClaimsIdentity cp)
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

            // Check if the user has the required roles (tenant-scoped)
            if (!cp.IsInRole($"{tenant}:Administrator") && !cp.IsInRole($"{tenant}:Instructor"))
            {
                logger.LogError("AddAsset called by user without sufficient permissions. UserId: {UserId}", cp.GetUserId());
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

            if (request.File.Length < 1)
            {
                logger.LogError("AddAsset called with an empty file");
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status400BadRequest,
                        "Bad Request",
                        "The file cannot be empty.",
                        "Ensure that the file is not empty before uploading.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            if (request.File.Length > 10 * 1024 * 1024) // 10 MB limit
            {
                logger.LogError("AddAsset called with a file larger than 10 MB");
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status400BadRequest,
                        "Bad Request",
                        "The file size exceeds the maximum allowed size of 10 MB.",
                        "Ensure that the file size is within the allowed limit before uploading.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            // Validate the file type
            string[] allowedFileTypes = ["image/jpeg", "image/png", "application/pdf"];
            if (!allowedFileTypes.Contains(request.File.ContentType))
            {
                logger.LogError("AddAsset called with an unsupported file type: {ContentType}", request.File.ContentType);
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status415UnsupportedMediaType,
                        "Unsupported Media Type",
                        "The file type is not supported.",
                        "Ensure that the file type is one of the allowed types: JPEG, PNG, PDF.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            // Retrieve the storage client
            if (storageClient == null)
            {
                logger.LogError("AddAsset failed to retrieve IStorageClient");
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "Failed to retrieve storage client.",
                        "Ensure that the storage client is configured correctly.",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            // Log the file upload attempt
            logger.LogInformation("AddAsset called by user {UserId} to upload file {FileName} with size {FileSize} bytes", cp.GetUserId(), request.File.FileName, request.File.Length);

            Asset? asset = null;

            //read the file into a stream
            try
            {
                // Upload the file to the storage client
                asset = await storageClient.UploadAssetAsync(request.File, "test");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to read the file stream for file {FileName}", request.File.FileName);
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "Internal Server Error",
                        "",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            if (asset == null)
            {
                logger.LogError("AddAsset failed to upload file {FileName} to storage", request.File.FileName);
                return TypedResults.BadRequest(
                    new ErrorResponse(
                        StatusCodes.Status500InternalServerError,
                        "Internal Server Error",
                        "Internal Server Error",
                        "",
                        httpContextAccessor.HttpContext?.TraceIdentifier
                    )
                );
            }

            // Return user profile
            return TypedResults.Ok<AddAssetResponse>(new(asset.Id, asset.Path));
        }
    }
}
