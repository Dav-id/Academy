using Academy.Shared.Data.Contexts;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

using System.Reflection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

builder.Services.AddHttpContextAccessor();

builder.AddNpgsqlDbContext<ApplicationDbContext>("Academy");

// // Add OpenID Connect
// ConfigurationManager<OpenIdConnectConfiguration> configManager = new($"{Configuration["Settings:AuthAuthority"]}", new OpenIdConnectConfigurationRetriever());
// OpenIdConnectConfiguration openidconfig = configManager.GetConfigurationAsync().Result;

// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// })
// .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateAudience = false,
//         ValidateIssuer = false,
//         ValidIssuer = openidconfig.Issuer,
//         ValidateActor = false,
//         ValidateIssuerSigningKey = true,
//         IssuerSigningKeys = openidconfig.SigningKeys
//     };

//     options.Events = new JwtBearerEvents()
//     {
//         OnAuthenticationFailed = context =>
//         {
//             if (context.Exception is SecurityTokenExpiredException)
//             {
//                 // if you end up here, you know that the token is expired
//                 context.Response.Headers.Append("Token-Expired", "true");
//             }

//             return Task.CompletedTask;
//         }
//     };
// });

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

// Add healthcheck endpoint. 
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Register our endpoints 

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
            if (method.GetParameters().Length != 2 ||
                method.GetParameters()[0].ParameterType != typeof(IEndpointRouteBuilder) ||
                method.GetParameters()[1].ParameterType != typeof(IServiceProvider))
            {
                // If the method signature is not as expected, throw an exception
                throw new InvalidOperationException($"{endpointType.DeclaringType.FullName} must implement RequiredMethod with the correct signature.");
            }

            if (endpointNames.Contains(endpointType.DeclaringType.FullName.Trim()))
            {
                // If the endpoint has already been registered, skip it
                continue;
            }

            // If the method is found, invoke it with the app and serviceProvider
            method.Invoke(null, [app, serviceProvider]);

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

// Start the application

app.Run();
