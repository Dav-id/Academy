using Academy.Shared.Security.Models;

using io.fusionauth.domain;

using Microsoft.Extensions.Logging;

using System.Data;
using System.Security.Cryptography;

namespace Academy.Shared.Security.FusionAuth
{
    public class FusionAuthClient(
        ILogger<FusionAuthClient> logger,
        string apiUrl,
        string apiKey,
        string tenantId,
        string audience,
        string issuer,
        ICollection<IdentityProviderRoleMapping> externalRoleMappings) : IAuthClient
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

        public string ProviderName => "FusionAuth";

        private static string GenerateSecurePassword(int length = 32) => RandomNumberGenerator.GetString(Allowed, length);

        public async Task<UserProfile?> CreateUserAsync(string firstName, string lastName, string email)
        {
            UserProfile? existingUser = await GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                return existingUser;
            }

            io.fusionauth.FusionAuthClient authClient = new(_apiKey, _apiUrl, _tenantId);

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
                    UserRegistration? registration = u.registrations.FirstOrDefault(x => x.applicationId == audience);
                    if (registration != null)
                    {
                        // Retrieve roles from the registration and map to our application's roles
                        if (registration.roles.Count > 0)
                        {
                            foreach (string? r in registration.roles)
                            {
                                IdentityProviderRoleMapping? role = _identityProviderRoleMappings.FirstOrDefault(x => x.Issuer == _issuer && x.ExternalClaimValue == r);
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

        public async Task CreateRoleAsync(string role)
        {
            try
            {
                io.fusionauth.FusionAuthClient authClient = new(_apiKey, _apiUrl, _tenantId);

                // Retrieve the application to add the role to
                io.fusionauth.ClientResponse<io.fusionauth.domain.api.ApplicationResponse> appResponse = await authClient.RetrieveApplicationAsync(Guid.Parse(_audience));
                if (!appResponse.WasSuccessful())
                {
                    _logger.LogError("Failed to retrieve application for role creation. Status: {Status}", appResponse.statusCode);
                    throw new Exception("Application not found for role creation.");
                }

                Application application = appResponse.successResponse.application;
                if (application.roles.Any(r => r.name == role))
                {
                    // Role already exists, nothing to do
                    return;
                }

                // Add the new role
                //application.roles.Add(new io.fusionauth.domain.ApplicationRole
                //{
                //    name = role,
                //    isDefault = false,
                //    isSuperRole = false
                //});

                //Dictionary<string, object> patch = new()
                //{
                //    { "roles",  }
                //};

                var updateResponse = await authClient.CreateApplicationRoleAsync(application.id, null, new()
                {
                    //application = application,
                    role = new()
                    {
                        name = role,
                        isDefault = false,
                        isSuperRole = false
                    }
                });

                //io.fusionauth.ClientResponse<io.fusionauth.domain.api.ApplicationResponse> updateResponse = await authClient.PatchApplicationAsync(application.id, patch);

                if (!updateResponse.WasSuccessful())
                {
                    _logger.LogError("Failed to create role '{Role}'. Status: {Status}", role, updateResponse.statusCode);
                    throw new Exception($"Failed to create role '{role}'.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating role '{Role}'", role);
                throw;
            }
        }

        public async Task AddUserToRoleAsync(string id, string role)
        {
            try
            {
                io.fusionauth.FusionAuthClient authClient = new(_apiKey, _apiUrl, _tenantId);

                // Retrieve the user
                io.fusionauth.ClientResponse<io.fusionauth.domain.api.UserResponse> userResponse = await authClient.RetrieveUserAsync(Guid.Parse(id));
                if (!userResponse.WasSuccessful())
                {
                    _logger.LogError("Failed to retrieve user '{UserId}' for role assignment. Status: {Status}", id, userResponse.statusCode);
                    throw new Exception("User not found for role assignment.");
                }

                User user = userResponse.successResponse.user;

                // Retrieve the application registration
                Guid appId = Guid.Parse(_audience);
                UserRegistration? registration = user.registrations.FirstOrDefault(r => r.applicationId == appId);
                if (registration == null)
                {
                    // Register the user to the application with the role
                    io.fusionauth.domain.api.user.RegistrationRequest regRequest = new()
                    {
                        registration = new io.fusionauth.domain.UserRegistration
                        {
                            applicationId = appId,
                            roles = [role]
                        }
                    };
                    io.fusionauth.ClientResponse<io.fusionauth.domain.api.user.RegistrationResponse> regResponse = await authClient.RegisterAsync(user.id, regRequest);
                    if (!regResponse.WasSuccessful())
                    {
                        _logger.LogError("Failed to register user '{UserId}' to application with role '{Role}'. Status: {Status}", id, role, regResponse.statusCode);
                        throw new Exception($"Failed to register user '{id}' to application with role '{role}'.");
                    }
                }
                else
                {
                    // Add the role if not already present
                    if (!registration.roles.Contains(role))
                    {
                        registration.roles.Add(role);
                        io.fusionauth.domain.api.user.RegistrationRequest regUpdateRequest = new()
                        {
                            registration = registration
                        };
                        io.fusionauth.ClientResponse<io.fusionauth.domain.api.user.RegistrationResponse> regUpdateResponse = await authClient.UpdateRegistrationAsync(user.id, regUpdateRequest);
                        if (!regUpdateResponse.WasSuccessful())
                        {
                            _logger.LogError("Failed to add role '{Role}' to user '{UserId}'. Status: {Status}", role, id, regUpdateResponse.statusCode);
                            throw new Exception($"Failed to add role '{role}' to user '{id}'.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while adding user '{UserId}' to role '{Role}'", id, role);
                throw;
            }
        }
    }
}