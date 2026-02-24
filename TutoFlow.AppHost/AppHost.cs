using Projects;

#pragma warning disable MA0048
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var mainDb = postgres.AddDatabase("tutoflow-dev");
var shardAlpha = postgres.AddDatabase("shard-alpha");
var shardBeta = postgres.AddDatabase("shard-beta");

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<TutoFlow_ApiService>("apiservice")
    .WithReference(mainDb)
    .WithReference(shardAlpha)
    .WithReference(shardBeta)
    .WaitFor(postgres)
    .WithHttpHealthCheck("/health");

builder.AddProject<TutoFlow_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync().ConfigureAwait(false);
