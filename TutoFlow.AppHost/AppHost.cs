using Projects;
using Serilog;
using System.Globalization;

#pragma warning disable MA0048
var builder = DistributedApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, lc) => lc
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        formatProvider: CultureInfo.InvariantCulture));

var postgres = builder.AddPostgres("postgres").WithDataVolume();
var mainDb = postgres.AddDatabase("tutoflow-dev");

var shardDundukPostgres = builder.AddPostgres("postgres-shard-dunduk").WithDataVolume();
var shardDunduk = shardDundukPostgres.AddDatabase("shard-dunduk");

var shardFundukPostgres = builder.AddPostgres("postgres-shard-funduk").WithDataVolume();
var shardFunduk = shardFundukPostgres.AddDatabase("shard-funduk");

var cache = builder.AddRedis("cache").WithDataVolume();

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
