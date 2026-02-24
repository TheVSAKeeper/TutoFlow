using Projects;

#pragma warning disable MA0048
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres");
var mainDb = postgres.AddDatabase("tutoflow-dev");

var shardDundukPostgres = builder.AddPostgres("postgres-shard-dunduk");
var shardDunduk = shardDundukPostgres.AddDatabase("shard-dunduk");

var shardFundukPostgres = builder.AddPostgres("postgres-shard-funduk");
var shardFunduk = shardFundukPostgres.AddDatabase("shard-funduk");

var cache = builder.AddRedis("cache");

var apiService = builder.AddProject<TutoFlow_ApiService>("apiservice")
    .WithReference(mainDb)
    .WithReference(shardDunduk)
    .WithReference(shardFunduk)
    .WaitFor(postgres)
    .WaitFor(shardDundukPostgres)
    .WaitFor(shardFundukPostgres)
    .WithHttpHealthCheck("/health");

builder.AddProject<TutoFlow_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

await builder.Build().RunAsync().ConfigureAwait(false);
