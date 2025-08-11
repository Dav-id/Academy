using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using System.Reflection;
using System.Threading.RateLimiting;

using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//-------------------------------------
// Get the secrets from Hashicorp Vault
//-------------------------------------

// Used by Vault for segregation of credentials
string academyInstance = builder.Configuration["academy-instance"] ?? "";

// Default to using Vault which can be launched from the separate docker-compose.yaml file. You can override this by setting it in the user secrets.
string vaultUrl = builder.Configuration["vault-url"] ?? "";
string vaultToken = builder.Configuration["vault-token"] ?? "";

// Initialize one of the several auth methods.
IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);

// Initialize settings. You can also set proxies, custom delegates etc. here.
VaultClientSettings vaultClientSettings = new(vaultUrl, authMethod);

VaultClient vaultClient = new(vaultClientSettings);

// We have a KeyValue secrets engine called kv-v2 with a secret matching the academy-instance and containing key value pairs below
Secret<SecretData> secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(academyInstance);

string authUrl = ((System.Text.Json.JsonElement)secret.Data.Data["auth-url"]).GetString() ?? "";
string authAudience = ((System.Text.Json.JsonElement)secret.Data.Data["auth-audience"]).GetString() ?? "";
string authIssuer = ((System.Text.Json.JsonElement)secret.Data.Data["auth-issuer"]).GetString() ?? "";
string authOpenIdConfiguration = ((System.Text.Json.JsonElement)secret.Data.Data["auth-openid-configuration"]).GetString() ?? "";

//-------------------------------------
// Build the application
//-------------------------------------

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddHttpContextAccessor();

builder.AddNpgsqlDbContext<ApplicationDbContext>("Academy");

// Add OpenID JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authUrl;
                    options.Audience = authAudience;
                    options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        // FusionAuth uses a custom OpenID Connect configuration endpoint for each tenant.
                        // We use this to ensure we are using the correct signing keys.
                        authOpenIdConfiguration,
                        new OpenIdConnectConfigurationRetriever()
                    );
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = authIssuer,
                        ValidateAudience = true,
                        ValidAudience = authAudience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true
                    };
                });

// Require authentication by default
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
});

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

// Add rate limiting services
// TODO: add distributed cache for rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Use user identity if authenticated, otherwise use IP address
        string user = httpContext.User?.Identity?.IsAuthenticated == true ? httpContext.User.Identity.Name ?? "anonymous" : httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Example: 100 requests per minute per user/IP
        return RateLimitPartition.GetTokenBucketLimiter(user, _ => new TokenBucketRateLimiterOptions
        {
            TokenLimit = 100,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0,
            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
            TokensPerPeriod = 100,
            AutoReplenishment = true
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

// Add healthcheck endpoint. 
app.MapHealthChecks("/health").AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add rate limiting middleware
app.UseRateLimiter();

//-------------------------------------
// Register our endpoints
//-------------------------------------

// get IServiceProvider for our Endpoint mapping, used for getting DB context etc. 
IServiceProvider serviceProvider = app.Services;

List<string> endpointNames = [];

// Map all endpoints in the Academy.Services.Api.Endpoints namespace
foreach (Type endpointType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                                                                     .Where(type => type.IsClass
                                                                                    && !type.IsAbstract
                                                                                    && type.Namespace?.StartsWith("Academy.Services.Api.Endpoints") == true))
{
    if (endpointType?.DeclaringType?.Name == "Endpoint")
    {
        MethodInfo? method = endpointType.DeclaringType.GetMethod("AddEndpoint", BindingFlags.Static | BindingFlags.Public);
        if (method == null)
        {
            // If the method is not found, throw an exception
            throw new InvalidOperationException($"{endpointType.DeclaringType.FullName} must implement RequiredMethod.");
        }
        else
        {
            if (method.GetParameters().Length != 1 ||
                method.GetParameters()[0].ParameterType != typeof(IEndpointRouteBuilder))
            {
                // If the method signature is not as expected, throw an exception
                throw new InvalidOperationException($"{endpointType.DeclaringType.FullName} must implement RequiredMethod with the correct signature.");
            }

            if (string.IsNullOrEmpty(endpointType.DeclaringType.FullName) || endpointNames.Contains(endpointType.DeclaringType.FullName.Trim()))
            {
                // If the endpoint has already been registered, skip it
                continue;
            }

            // If the method is found, invoke it with the app and serviceProvider
            method.Invoke(null, [app]);

            // Add to our list so we register it only once
            endpointNames.Add(endpointType.DeclaringType.FullName.Trim());

            #region Log the routes

            app.Logger.LogInformation("Registered endpoint: {class}", endpointType.DeclaringType.FullName);

            //List<string> routes = endpoints.GetValue();
            FieldInfo? field = endpointType.DeclaringType.GetField("Routes", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (field == null || field.FieldType != typeof(List<string>))
            {
                throw new InvalidOperationException("Field not found or not of type List<string>");
            }

            List<string> routes = (List<string>)field.GetValue(null)!;

            //log the routes in a single LogInformation
            if (routes.Count > 0)
            {
                app.Logger.LogInformation("Registered routes: {routes}", string.Join(",\n ", routes));
            }
            else
            {
                app.Logger.LogInformation("No routes registered for this endpoint.");
            }

            #endregion
        }
    }
}

endpointNames.Clear();

//-------------------------------------
// Start the application
//-------------------------------------
app.Run();
