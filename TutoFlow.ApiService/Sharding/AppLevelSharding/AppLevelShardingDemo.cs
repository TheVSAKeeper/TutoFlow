#pragma warning disable MA0048, MA0051, CA1515
using Microsoft.EntityFrameworkCore;
using TutoFlow.ApiService.Data.Models;

namespace TutoFlow.ApiService.Sharding.AppLevelSharding;

internal sealed class ShardDbContext(DbContextOptions<ShardDbContext> options) : DbContext(options)
{
    public DbSet<Center> Centers => Set<Center>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfiguration(new CenterConfiguration());

        modelBuilder.Ignore<AdminProfile>();
        modelBuilder.Ignore<CenterMembership>();
        modelBuilder.Ignore<TutorProfile>();
        modelBuilder.Ignore<User>();
        modelBuilder.Ignore<ClientProfile>();
        modelBuilder.Ignore<Student>();
    }
}

internal sealed record ShardStats(string ShardName, int RowCount, string[] CenterNames);

internal sealed record ShardCenterInfo(int Id, string Name, string? Address, string ShardName);

internal sealed class ShardManager
{
    private static readonly string[] ShardNames = ["shard-dunduk", "shard-funduk"];
    private readonly Dictionary<string, string> _shardConnectionStrings = new(StringComparer.Ordinal);

    public static IReadOnlyList<string> AllShards => ShardNames;

    public static string ResolveShardName(string name)
    {
        var hash = (uint)name.GetHashCode(StringComparison.Ordinal);
        return ShardNames[hash % 2];
    }

    public void RegisterShard(string shardName, string connectionString)
    {
        _shardConnectionStrings[shardName] = connectionString;
    }

    public ShardDbContext CreateContext(string shardName)
    {
        if (!_shardConnectionStrings.TryGetValue(shardName, out var connStr))
        {
            throw new InvalidOperationException($"Шард '{shardName}' не зарегистрирован");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ShardDbContext>();
        optionsBuilder.UseNpgsql(connStr);

        return new(optionsBuilder.Options);
    }

    public async Task InitializeShardsAsync()
    {
        foreach (var shardName in ShardNames)
        {
            var ctx = CreateContext(shardName);
            await using var _ = ctx.ConfigureAwait(false);
            await ctx.Database.EnsureCreatedAsync().ConfigureAwait(false);
        }
    }
}

internal static class AppLevelShardingDemo
{
    public static void MapAppLevelShardingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sharding/app-level");

        group.MapPost("/init", InitAsync)
            .WithName("InitAppLevelSharding")
            .WithDescription("Инициализирует таблицы на обоих шардах");

        group.MapPost("/seed", SeedAsync)
            .WithName("SeedAppLevelSharding")
            .WithDescription("Создаёт demo-центры, маршрутизируя по hash(name) на разные шарды");

        group.MapPost("/add", AddCenterAsync)
            .WithName("AddAppLevelCenter")
            .WithDescription("Добавляет один центр — шард определяется по hash(name)");

        group.MapGet("/stats", GetStatsAsync)
            .WithName("GetAppLevelStats")
            .WithDescription("Возвращает количество центров на каждом шарде");

        group.MapGet("/query", QueryCenterAsync)
            .WithName("QueryAppLevelCenter")
            .WithDescription("Находит центр по имени на нужном шарде");

        group.MapDelete("/reset", ResetAsync)
            .WithName("ResetAppLevelSharding")
            .WithDescription("Очищает данные на обоих шардах");
    }

    private static async Task<IResult> InitAsync(ShardManager shardManager)
    {
        await shardManager.InitializeShardsAsync().ConfigureAwait(false);

        return Results.Ok(new
        {
            Message = "Схема инициализирована на всех шардах",
            Shards = ShardManager.AllShards,
        });
    }

    private static async Task<IResult> SeedAsync(ShardManager shardManager, int count = 20)
    {
        var created = new List<ShardCenterInfo>();

        for (var i = 1; i <= count; i++)
        {
            var centerName = $"Центр «{GetCenterName(i)}» #{i}";
            var shardName = ShardManager.ResolveShardName(centerName);
            var ctx = shardManager.CreateContext(shardName);
            await using var _1 = ctx.ConfigureAwait(false);

            var center = new Center
            {
                Name = centerName,
                Address = $"г. Москва, ул. Примерная, д. {i}",
                Phone = $"+7-495-{i:D3}-{i * 17 % 1000:D3}-{i * 31 % 100:D2}",
                Email = $"center{i}@tutoflow.ru",
            };

            ctx.Centers.Add(center);
            await ctx.SaveChangesAsync().ConfigureAwait(false);

            created.Add(new(center.Id, center.Name, center.Address, shardName));
        }

        return Results.Ok(new { InsertedCount = created.Count, Centers = created });
    }

    private static async Task<IResult> AddCenterAsync(ShardManager shardManager, string name, string? address = null)
    {
        var shardName = ShardManager.ResolveShardName(name);
        var ctx = shardManager.CreateContext(shardName);
        await using var _ = ctx.ConfigureAwait(false);

        var center = new Center
        {
            Name = name,
            Address = address,
        };

        ctx.Centers.Add(center);
        await ctx.SaveChangesAsync().ConfigureAwait(false);

        return Results.Ok(new ShardCenterInfo(center.Id, center.Name, center.Address, shardName));
    }

    private static async Task<IResult> GetStatsAsync(ShardManager shardManager)
    {
        var stats = new List<ShardStats>();

        foreach (var shardName in ShardManager.AllShards)
        {
            var ctx = shardManager.CreateContext(shardName);
            await using var _2 = ctx.ConfigureAwait(false);
            var centers = await ctx.Centers.OrderBy(c => c.Id).ToListAsync().ConfigureAwait(false);

            stats.Add(new(shardName, centers.Count, [.. centers.Select(c => $"[{c.Id}] {c.Name}")]));
        }

        return Results.Ok(new { TotalCenters = stats.Sum(s => s.RowCount), Stats = stats });
    }

    private static async Task<IResult> QueryCenterAsync(ShardManager shardManager, string name)
    {
        var shardName = ShardManager.ResolveShardName(name);
        var ctx = shardManager.CreateContext(shardName);
        await using var _ = ctx.ConfigureAwait(false);

        var center = await ctx.Centers.FirstOrDefaultAsync(c => c.Name == name).ConfigureAwait(false);

        if (center is null)
        {
            return Results.NotFound(new { Message = $"Центр «{name}» не найден на шарде {shardName}" });
        }

        return Results.Ok(new ShardCenterInfo(center.Id, center.Name, center.Address, shardName));
    }

    private static async Task<IResult> ResetAsync(ShardManager shardManager)
    {
        var deleted = 0;

        foreach (var shardName in ShardManager.AllShards)
        {
            var ctx = shardManager.CreateContext(shardName);
            await using var _3 = ctx.ConfigureAwait(false);
            var count = await ctx.Centers.CountAsync().ConfigureAwait(false);
            ctx.Centers.RemoveRange(ctx.Centers);
            await ctx.SaveChangesAsync().ConfigureAwait(false);
            deleted += count;
        }

        return Results.Ok(new { Message = $"Удалено {deleted} центров со всех шардов" });
    }

    private static string GetCenterName(int index)
    {
        return (index % 10) switch
        {
            0 => "Знание",
            1 => "Эрудит",
            2 => "Прогресс",
            3 => "Олимп",
            4 => "Гармония",
            5 => "Интеллект",
            6 => "Перспектива",
            7 => "Академия",
            8 => "Горизонт",
            _ => "Успех",
        };
    }
}
