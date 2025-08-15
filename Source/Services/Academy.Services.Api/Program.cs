using Academy.Shared.Data.Contexts;
using Academy.Shared.Data.Models.Roles;
using Academy.Shared.Security;
using Academy.Shared.Security.FusionAuth;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerUI;

using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
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

string authUrl = ((JsonElement)secret.Data.Data["auth-url"]).GetString() ?? "";
string authAudience = ((JsonElement)secret.Data.Data["auth-audience"]).GetString() ?? "";
string authIssuer = ((JsonElement)secret.Data.Data["auth-issuer"]).GetString() ?? "";
string authOpenIdConfiguration = ((JsonElement)secret.Data.Data["auth-openid-configuration"]).GetString() ?? "";

string redisConnectionString = ((JsonElement)secret.Data.Data["redis-connection-string"]).GetString() ?? "";

//-------------------------------------
// Build the application
//-------------------------------------

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddHttpContextAccessor();

builder.AddNpgsqlDbContext<ApplicationDbContext>("Academy");


// Add Redis distributed cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "Academy:" + academyInstance + ":";
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

// Add OpenID JWT Auth
JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

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
                        ValidateIssuerSigningKey = true,
                    };

                    // Map the roles provided by the IdP to ASP.NET Core roles so we can allow enterprise roles
                    // to be used in our policies if they are not a direct match.
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async ctx =>
                        {
                            HttpContext http = ctx.HttpContext;
                            IDistributedCache cache = http.RequestServices.GetRequiredService<IDistributedCache>();

                            ClaimsPrincipal principal = ctx.Principal!;
                            ClaimsIdentity identity = principal.Identity as ClaimsIdentity ?? new ClaimsIdentity();

                            // Identify the issuer to support multiple providers/tenants
                            string? issuer = principal.FindFirst("iss")?.Value ?? principal.FindFirst(JwtRegisteredClaimNames.Iss)?.Value;
                            if (string.IsNullOrEmpty(issuer))
                            {
                                // No issuer? Play it safe: do not grant roles.
                                return;
                            }

                            // Load mapping for this issuer from cache/DB
                            string cacheKey = $"rolemap:{issuer}";

                            string cached = await cache.GetStringAsync(cacheKey) ?? string.Empty;
                            List<ExternalRoleMapping> mappings = [];

                            if (cached != null && !string.IsNullOrEmpty(cached))
                            {
                                mappings = JsonSerializer.Deserialize<List<ExternalRoleMapping>>(cached) ?? [];
                            }
                            else
                            {
                                ApplicationDbContext db = http.RequestServices.GetRequiredService<ApplicationDbContext>();
                                mappings = await db.ExternalRoleMappings
                                                    .Where(m => m.Issuer == issuer)
                                                    .ToListAsync(ctx.HttpContext.RequestAborted);

                                // Store in cache for 5 minutes
                                DistributedCacheEntryOptions options = new()
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
                                };

                                await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(mappings), options);
                            }

                            if (mappings.Count == 0)
                            {
                                return; // nothing to map
                            }

                            // Build a fast lookup: (type,value) -> app roles[]
                            Dictionary<string, string[]> map = mappings.GroupBy(m => $"{m.ExternalClaimType}:{m.ExternalClaimValue}")
                                                                       .ToDictionary(
                                                                           g => g.Key,
                                                                           g => g.Select(x => x.AppRole).Distinct().ToArray(),
                                                                           StringComparer.OrdinalIgnoreCase
                                                                       );

                            // Collect external claims we care about (roles/groups/permissions or custom)
                            List<Claim> allClaims = [.. principal.Claims];

                            HashSet<string> matchedAppRoles = new(StringComparer.OrdinalIgnoreCase);

                            foreach (Claim c in allClaims)
                            {
                                if (map.TryGetValue($"{c.Type}:{c.Value}", out string[]? appRoles))
                                {
                                    foreach (string r in appRoles)
                                    {
                                        matchedAppRoles.Add(r);
                                    }
                                }


                            }

                            // Attach mapped app roles as role claims understood by ASP.NET authorization
                            // Use the same role claim type that authorization uses (default ClaimTypes.Role or your configured RoleClaimType)
                            string roleClaimType = identity.RoleClaimType; // aligns with TokenValidationParameters.RoleClaimType
                            foreach (string role in matchedAppRoles)
                            {
                                // Avoid duplicates
                                if (!principal.Claims.Any(cl => cl.Type == roleClaimType && cl.Value.Equals(role, StringComparison.OrdinalIgnoreCase)))
                                {
                                    identity.AddClaim(new Claim(roleClaimType, role));
                                }
                            }
                        }
                    };
                });

// Require authentication by default
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrator", p => p.RequireRole("Administrator"));
    options.AddPolicy("Instructor", p => p.RequireAssertion(ctx => ctx.User.IsInRole("Administrator") || ctx.User.IsInRole("Instructor")));
    options.AddPolicy("Learner", p => p.RequireAssertion(ctx => ctx.User.IsInRole("Administrator") || ctx.User.IsInRole("Instructor") || ctx.User.IsInRole("Learner")));

    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
});

// Add services to the container.
builder.Services.AddProblemDetails();

// Add localization services
builder.Services.AddLocalization();

// Configure supported cultures (currently English and Portuguese)
CultureInfo[] supportedCultures = [
    new CultureInfo("en"),
    new CultureInfo("pt"),
];

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;     // for numbers, dates
    options.SupportedUICultures = supportedCultures;   // for UI/resources

    options.RequestCultureProviders = [
        new QueryStringRequestCultureProvider(),       // ?culture=pt&ui-culture=pt        
        new AcceptLanguageHeaderRequestCultureProvider()
    ];

    options.ApplyCurrentCultureToResponseHeaders = true;
});


// Add FluentValidation services
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

// Add OpenAPI/Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Academy.Services.API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

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

// Add Auth Client
// TODO: In the future select the correct client based on the auth provider
// TODO: Add Azure AD, Auth0, etc. clients
builder.Services.AddSingleton<IAuthClient, FusionAuthClient>();

WebApplication app = builder.Build();

//Auto Migrate DB
using (IServiceScope serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    ApplicationDbContext? context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

    if (context != null)
    {
        context.Database.EnsureCreated();
        context.Database.Migrate();
    }

    //Initialise the Auth Client (run the async setup)
    IAuthClient authClient = serviceScope.ServiceProvider.GetRequiredService<IAuthClient>();
    await authClient.Initialise();
}

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

// Add healthcheck endpoint. 
app.MapHealthChecks("/health").AllowAnonymous();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Academy.Services.API V1");
        c.RoutePrefix = "api/docs";
        c.EnableDeepLinking();
        c.EnableFilter();
        c.DocExpansion(DocExpansion.None);
        c.DefaultModelExpandDepth(0);
    });
}

// Add rate limiting middleware
app.UseRateLimiter();

// Add localization middleware
app.UseRequestLocalization();

//-------------------------------------
// Register our endpoints
//-------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapGet("/me", (ClaimsPrincipal user) =>
    {
        //string name = user.Identity.Name ?? user.FindFirst("preferred_username")?.Value ?? "unknown";
        string id = user.FindFirst(ClaimTypes.Sid)?.Value ?? user.FindFirst("sid")?.Value ?? "unknown";
        string roles = string.Join(", ", user.FindAll(ClaimTypes.Role).Select(c => c.Value));
        return Results.Ok(new { id, roles });
    });
}

List<string> endpointNames = [];

// Map all endpoints in the Academy.Services.Api.Endpoints namespace
foreach (Type endpointType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                                                                        .Where(type => type.IsClass
                                                                                    && !type.IsAbstract
                                                                                    && type.Namespace?.StartsWith("Academy.Services.Api.Endpoints") == true))
{
    if (endpointType?.DeclaringType?.Name.EndsWith("Endpoint") ?? false)
    {
        MethodInfo? method = endpointType.DeclaringType.GetMethod("AddEndpoint", BindingFlags.Static | BindingFlags.Public);
        if (method == null)
        {
            // If the method is not found, throw an exception
            throw new InvalidOperationException($"{endpointType.DeclaringType.FullName} must implement RequiredMethod.");
        }
        else
        {
            if (method.GetParameters().Length != 1 || method.GetParameters()[0].ParameterType != typeof(IEndpointRouteBuilder))
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

//-------------------------------------
// Start the application
//-------------------------------------
app.Run();
