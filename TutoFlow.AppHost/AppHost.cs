using Projects;

#pragma warning disable MA0048
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var mainDb = postgres.AddDatabase("tutoflow-dev");

var shardAlphaPostgres = builder.AddPostgres("postgres-shard-alpha");
var shardAlpha = shardAlphaPostgres.AddDatabase("shard-alpha");

var shardBetaPostgres = builder.AddPostgres("postgres-shard-beta");
var shardBeta = shardBetaPostgres.AddDatabase("shard-beta");

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<TutoFlow_ApiService>("apiservice")
    .WithReference(mainDb)
    .WithReference(shardAlpha)
    .WithReference(shardBeta)
    .WaitFor(postgres)
    .WaitFor(shardAlphaPostgres)
    .WaitFor(shardBetaPostgres)
    .WithHttpHealthCheck("/health");

builder.AddProject<TutoFlow_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync().ConfigureAwait(false);
