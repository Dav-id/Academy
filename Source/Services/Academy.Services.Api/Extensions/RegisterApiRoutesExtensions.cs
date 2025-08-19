using System.Reflection;

namespace Academy.Services.Api.Middleware
{
    /// <summary>
    /// Middleware to resolve tenant ID from the tenant url stub in the request and set it in the database context.
    /// </summary>

    public static class RegisterApiRoutesExtensions
    {
        public static void RegisterApiRoutes(this WebApplication app)
        {
            List<string> endpointNames = [];

            // Map all endpoints in the Academy.Services.Api.Endpoints namespace
            foreach (Type endpointType in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                                                                                    .Where(type => type.IsClass
                                                                                                && !type.IsAbstract
                                                                                                && type.Namespace?.StartsWith("Academy.Services.Api.Endpoints") == true))
            {
                if (endpointType?.DeclaringType?.Name.EndsWith("Endpoints") ?? false)
                {
                    MethodInfo? method = endpointType.DeclaringType.GetMethod("AddEndpoints", BindingFlags.Static | BindingFlags.Public);
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
                            app.Logger.LogInformation("Registered routes:\n {routes}", string.Join(",\n ", routes));
                        }
                        else
                        {
                            app.Logger.LogInformation("No routes registered for this endpoint.");
                        }

                        #endregion
                    }
                }
            }
        }
    }
}
