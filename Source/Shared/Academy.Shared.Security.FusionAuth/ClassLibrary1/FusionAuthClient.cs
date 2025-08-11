using Academy.Shared.Data.Contexts;
using Academy.Shared.Security.Models;

using io.fusionauth.domain;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System.Security.Cryptography;
using System.Text.Json;

using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

namespace Academy.Shared.Security.FusionAuth
{
    public class FusionAuthClient(IConfiguration configuration, ILogger<FusionAuthClient> logger, IServiceScopeFactory serviceScopeFactory) : IAuthClient
    {
        private readonly ILogger<FusionAuthClient> _logger = logger;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

        private readonly string _academyInstance = configuration["academy-instance"] ?? "";
        private readonly string _vaultUrl = configuration["vault-url"] ?? "";
        private readonly string _vaultToken = configuration["vault-token"] ?? "";

        private string _apiKey = string.Empty;
        private string _apiUrl = string.Empty;
        private string _tenantId = string.Empty;
        private string _audience = string.Empty;
        private string _issuer = string.Empty;

        public async Task Initialise()
        {
            // Ensure the Vault client is initialized and can connect to the Vault server.
            try
            {
                // Initialize settings. You can also set proxies, custom delegates etc. here.
                VaultClientSettings vaultClientSettings = new(_vaultUrl, new TokenAuthMethodInfo(_vaultToken));

                VaultClient vaultClient = new(vaultClientSettings);
                Secret<SecretData> secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(_academyInstance);

                _apiKey = ((JsonElement)secret.Data.Data["auth-fusion-apikey"]).GetString() ?? "";
                _apiUrl = ((JsonElement)secret.Data.Data["auth-fusion-apiurl"]).GetString() ?? "";
                _tenantId = ((JsonElement)secret.Data.Data["auth-fusion-tenantid"]).GetString() ?? "";
                
                _audience = ((JsonElement)secret.Data.Data["auth-audience"]).GetString() ?? "";
                _issuer = ((JsonElement)secret.Data.Data["auth-issuer"]).GetString() ?? "";

                if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_apiUrl) || string.IsNullOrEmpty(_tenantId))
                {
                    throw new InvalidOperationException("FusionAuth API key, URL, or tenant ID is not configured properly.");
                }

                if (string.IsNullOrEmpty(_audience) || string.IsNullOrEmpty(_issuer))
                {
                    throw new InvalidOperationException("Audience or issuer is not configured properly.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to connect to Vault server.", ex);
            }
        }

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
            io.fusionauth.FusionAuthClient authClient = new io.fusionauth.FusionAuthClient(_apiKey, _apiUrl, _tenantId);

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
                            await using var scope = _serviceScopeFactory.CreateAsyncScope();
                            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                            foreach(var r in registration.roles)
                            {
                                var role = db.ExternalRoleMappings.FirstOrDefault(x => x.Issuer == _issuer && x.ExternalClaimValue == r);
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
