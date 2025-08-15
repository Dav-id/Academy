using Academy.Shared.Security.Models;

using io.fusionauth.domain;

using Microsoft.Extensions.Logging;

using System.Security.Cryptography;

namespace Academy.Shared.Security.FusionAuth
{
    public class FusionAuthClient(ILogger<FusionAuthClient> logger, string apiUrl, string apiKey, string tenantId, string audience, string issuer, ICollection<IdentityProviderRoleMapping> externalRoleMappings) : IAuthClient
    {
        private readonly ILogger<FusionAuthClient> _logger = logger;

        private readonly string _apiUrl = apiUrl;
        private readonly string _apiKey = apiKey;
        private readonly string _tenantId = tenantId;
        private readonly string _audience = audience;
        private readonly string _issuer = issuer;
        private readonly ICollection<IdentityProviderRoleMapping> _identityProviderRoleMappings = externalRoleMappings;

        private const string Allowed =
            "ABCDEFGHJKLMNPQRSTUVWXYZ" +
            "abcdefghijkmnopqrstuvwxyz" +
            "23456789" +
            "!@#$%^&*_-+=?";

        private static string GenerateSecurePassword(int length = 32) => RandomNumberGenerator.GetString(Allowed, length);

        public async Task<UserProfile?> CreateUserAsync(string firstName, string lastName, string email)
        {
            UserProfile? existingUser = await GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                return existingUser;
            }

            io.fusionauth.FusionAuthClient authClient = new io.fusionauth.FusionAuthClient(_apiKey, _apiUrl, _tenantId);

            io.fusionauth.ClientResponse<io.fusionauth.domain.api.UserResponse> cu = await authClient.CreateUserAsync(null, new io.fusionauth.domain.api.UserRequest
            {
                user = new io.fusionauth.domain.User()
                {
                    email = email,
                    username = email,
                    password = GenerateSecurePassword()
                }
            });

            if (cu.WasSuccessful())
            {
                User u = cu.successResponse.user;
                return new UserProfile(u.id.ToString() ?? string.Empty, u.firstName, u.lastName, u.email, u.active ?? false, []);
            }
            else
            {
                if (cu?.errorResponse != null && cu.errorResponse?.fieldErrors.Count > 0)
                {
                    _logger.LogError("Failed to create user with email: {Email}. Status Code: {StatusCode}, Message: {Message}",
                        email, cu.statusCode, string.Join(";", cu.errorResponse?.fieldErrors.SelectMany(x => x.Key + ": " + x.Value) ?? "") ?? "No error message provided");
                }
                if (cu?.errorResponse != null && cu.errorResponse?.generalErrors.Count > 0)
                {
                    _logger.LogError("Failed to create user with email: {Email}. Status Code: {StatusCode}, Message: {Message}",
                        email, cu.statusCode, string.Join(";", cu.errorResponse?.generalErrors.SelectMany(x => x.message) ?? ""));
                }
                else
                {
                    _logger.LogError("Failed to create user with email: {Email}. Status Code: {StatusCode}, Message: {Message}",
                        email, cu?.statusCode, "No error message provided");
                }
            }
            return null;
        }

        public async Task<UserProfile?> GetUserByEmailAsync(string email)
        {
            io.fusionauth.FusionAuthClient authClient = new(_apiKey, _apiUrl, _tenantId);

            io.fusionauth.ClientResponse<io.fusionauth.domain.api.UserResponse> user = await authClient.RetrieveUserByEmailAsync(email);

            if (user.WasSuccessful())
            {
                User u = user.successResponse.user;

                List<string> roles = [];
                if (u.registrations.Count > 0 && Guid.TryParse(_audience, out Guid audience))
                {
                    var registration = u.registrations.FirstOrDefault(x => x.applicationId == audience);
                    if (registration != null)
                    {
                        // Retrieve roles from the registration and map to our application's roles
                        if (registration.roles.Count > 0)
                        {
                            foreach (var r in registration.roles)
                            {
                                var role = _identityProviderRoleMappings.FirstOrDefault(x => x.Issuer == _issuer && x.ExternalClaimValue == r);
                                if (role != null)
                                {
                                    roles.Add(role.AppRole);
                                }
                            }
                        }
                    }
                }

                return new UserProfile(u.id.ToString() ?? string.Empty, u.firstName, u.lastName, u.email, u.active ?? false, [.. roles]);
            }
            if (!user.WasSuccessful() && user != null)
            {
                if (user?.errorResponse != null && user.errorResponse?.fieldErrors.Count > 0)
                {
                    _logger.LogError("Failed to retrieve user by email: {Email}. Status Code: {StatusCode}, Message: {Message}",
                        email, user.statusCode, string.Join(";", user.errorResponse?.fieldErrors.SelectMany(x => x.Key + ": " + x.Value) ?? "") ?? "No error message provided");
                }
                if (user?.errorResponse != null && user.errorResponse?.generalErrors.Count > 0)
                {
                    _logger.LogError("Failed to retrieve user by email: {Email}. Status Code: {StatusCode}, Message: {Message}",
                        email, user.statusCode, string.Join(";", user.errorResponse?.generalErrors.SelectMany(x => x.message) ?? ""));
                }
                else
                {
                    _logger.LogError("Failed to retrieve user by email: {Email}. Status Code: {StatusCode}, Message: {Message}",
                        email, user?.statusCode, "No error message provided");
                }
            }

            return null;
        }
    }
}
