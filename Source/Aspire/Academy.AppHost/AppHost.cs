using VaultSharp;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.Commons;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

//-------------------------------------
// Get the secrets from Hashicorp Vault
//-------------------------------------

// Used by Vault for segregation of credentials
IResourceBuilder<ParameterResource> academyInstance = builder.AddParameter("academy-instance", "development", publishValueAsDefault: false, secret: true);

// Default to using Vault which can be launched from the separate docker-compose.yaml file. You can override this by setting it in the user secrets.
IResourceBuilder<ParameterResource> vaultUrl = builder.AddParameter("vault-url", "http://host.docker.internal:8200", publishValueAsDefault: false, secret: true);
IResourceBuilder<ParameterResource> vaultToken = builder.AddParameter("vault-token", secret: true);

// Initialize one of the several auth methods.
IAuthMethodInfo authMethod = new TokenAuthMethodInfo(await vaultToken.Resource.GetValueAsync(default) ?? "");

// Initialize settings. You can also set proxies, custom delegates etc. here.
VaultClientSettings vaultClientSettings = new(await vaultUrl.Resource.GetValueAsync(default) ?? "", authMethod);

VaultClient vaultClient = new(vaultClientSettings);

// We have a KeyValue secrets engine called kv-v2 with a secret matching the academy-instance and containing key value pairs below
Secret<SecretData> secret = await vaultClient.V1.Secrets.KeyValue.V2.ReadSecretAsync(await academyInstance.Resource.GetValueAsync(default));
string dbUsernameSecret = ((System.Text.Json.JsonElement)secret.Data.Data["db-username"]).GetString() ?? "";
string dbPasswordSecret = ((System.Text.Json.JsonElement)secret.Data.Data["db-password"]).GetString() ?? "";

if (string.IsNullOrEmpty(dbUsernameSecret) || string.IsNullOrEmpty(dbPasswordSecret))
{
    throw new Exception("username or password cannot be null or empty");
}

// Inject our Postgres credentials
IResourceBuilder<ParameterResource> username = builder.AddParameter("db-username", dbUsernameSecret, publishValueAsDefault: false, secret: true);
IResourceBuilder<ParameterResource> password = builder.AddParameter("db-password", dbPasswordSecret, publishValueAsDefault: false, secret: true);

// Add more secrets here as we require them

//-------------------
// Start the services
//-------------------

// Redis Cache
IResourceBuilder<RedisResource> cache = builder.AddRedis("cache");

// Postgres
IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres", username, password)
                                                           .WithDataVolume(isReadOnly: false);
// Create the DB
IResourceBuilder<PostgresDatabaseResource> postgresdb = postgres.AddDatabase("Academy");

// API Service
IResourceBuilder<ProjectResource> apiService = builder.AddProject<Projects.Academy_Services_Api>("services-api")
                                                      .WithReference(postgresdb)
                                                      .WithHttpHealthCheck("/health")
                                                      .WithReference(cache)
                                                      .WaitFor(cache)
                                                      ;

// Start it all
builder.Build().Run();
