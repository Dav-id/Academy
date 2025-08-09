using Aspire.Hosting;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ParameterResource> vaultToken = builder.AddParameter("vault-token", secret: true);

IResourceBuilder<ContainerResource> vault = builder.AddContainer("vault", "hashicorp/vault:latest")
        .WithEndpoint(8200, 8200, name: "vault-http")
        .WithEnvironment("VAULT_DEV_ROOT_TOKEN_ID", vaultToken)
        .WithEnvironment("VAULT_DEV_LISTEN_ADDRESS", "0.0.0.0:8200")
//.WithBindMount("/vault/config", "./vault-config");
//.WithHttpHealthCheck("http://localhost:8200/v1/sys/health", TimeSpan.FromSeconds(30));
;

IResourceBuilder<ParameterResource> username = builder.AddParameter("db-username", secret: true);
IResourceBuilder<ParameterResource> password = builder.AddParameter("db-password", secret: true);

IResourceBuilder<PostgresServerResource> postgres = builder.AddPostgres("postgres", username, password)
                                                           .WithDataVolume(isReadOnly: false);

IResourceBuilder<PostgresDatabaseResource> postgresdb = postgres.AddDatabase("Academy");

IResourceBuilder<RedisResource> cache = builder.AddRedis("cache");

IResourceBuilder<ProjectResource> apiService = builder.AddProject<Projects.Academy_Services_Api>("services-api")
                                                      .WithReference(postgresdb)
                                                      .WithHttpHealthCheck("/health")
                                                      .WithReference(cache)
                                                      .WaitFor(cache);

//builder.AddProject<Projects.Academy_Web>("clients-web")
//       .WithExternalHttpEndpoints()
//       .WithHttpHealthCheck("/health")
//       .WithReference(cache)
//       .WaitFor(cache)
//       .WithReference(apiService)
//       .WaitFor(apiService);

builder.Build().Run();
