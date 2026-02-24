#pragma warning disable MA0048, CA1515
using Microsoft.EntityFrameworkCore;
using TutoFlow.ApiService.Sharding.AppLevelSharding;
using TutoFlow.ApiService.Sharding.InterceptorSharding;
using TutoFlow.ApiService.Sharding.NativePartitioning;

namespace TutoFlow.ApiService.Sharding;

internal static class ShardingDemoExtensions
{
    public static void AddShardingDemos(this WebApplicationBuilder builder)
    {
        var shardManager = new ShardManager();

        var shardAlphaConnStr = builder.Configuration.GetConnectionString("shard-alpha");
        var shardBetaConnStr = builder.Configuration.GetConnectionString("shard-beta");

        if (!string.IsNullOrEmpty(shardAlphaConnStr))
        {
            shardManager.RegisterShard("shard-alpha", shardAlphaConnStr);
        }

        if (!string.IsNullOrEmpty(shardBetaConnStr))
        {
            shardManager.RegisterShard("shard-beta", shardBetaConnStr);
        }

        builder.Services.AddSingleton(shardManager);

        var mainConnStr = builder.Configuration.GetConnectionString("tutoflow-dev");

        builder.Services.AddDbContext<InterceptorShardingDbContext>(options =>
        {
            options.UseNpgsql(mainConnStr);
            options.AddInterceptors(new ShardRoutingInterceptor());
        });
    }

    public static void MapShardingDemos(this WebApplication app)
    {
        app.MapNativePartitioningEndpoints();
        app.MapAppLevelShardingEndpoints();
        app.MapInterceptorShardingEndpoints();
    }
}
