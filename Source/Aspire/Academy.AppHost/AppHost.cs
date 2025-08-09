IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

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
