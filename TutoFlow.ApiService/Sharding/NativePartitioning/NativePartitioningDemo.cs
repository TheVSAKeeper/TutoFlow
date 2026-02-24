#pragma warning disable MA0048, MA0051, CA1515, EF1002, S3267
using Microsoft.EntityFrameworkCore;
using TutoFlow.ApiService.Data;

namespace TutoFlow.ApiService.Sharding.NativePartitioning;

internal sealed record PartitionStats(string PartitionName, long RowCount);

internal sealed record SeedResult(int InsertedCount, string Message);

internal static class NativePartitioningDemo
{
    private const string PartitionedTable = "users_partitioned";
    private const int PartitionCount = 4;

    public static void MapNativePartitioningEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sharding/partitioning");

        group.MapPost("/init", InitializePartitionsAsync)
            .WithName("InitPartitioning")
            .WithDescription("Создаёт партицированную таблицу users_partitioned с 4 hash-партициями");

        group.MapPost("/seed", SeedDataAsync)
            .WithName("SeedPartitionedUsers")
            .WithDescription("Заполняет партицированную таблицу demo-данными");

        group.MapPost("/add", AddUserAsync)
            .WithName("AddPartitionedUser")
            .WithDescription("Добавляет одного пользователя с указанным email и ролью");

        group.MapGet("/stats", GetPartitionStatsAsync)
            .WithName("GetPartitionStats")
            .WithDescription("Возвращает количество строк в каждой партиции");

        group.MapDelete("/reset", ResetAsync)
            .WithName("ResetPartitioning")
            .WithDescription("Удаляет партицированную таблицу");
    }

    private static async Task<IResult> InitializePartitionsAsync(ApplicationDbContext db)
    {
        var exists = await db.Database.SqlQueryRaw<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{PartitionedTable}') AS \"Value\"")
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (exists)
        {
            return Results.Ok(new { Message = $"Таблица {PartitionedTable} уже существует" });
        }

        await db.Database.ExecuteSqlRawAsync("""
                                             DO $$
                                             BEGIN
                                                 IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'user_role') THEN
                                                     CREATE TYPE user_role AS ENUM ('client', 'tutor', 'admin', 'super_admin');
                                                 END IF;
                                             END$$;
                                             """)
            .ConfigureAwait(false);

        var sql = $"""
                   CREATE TABLE {PartitionedTable} (
                       id INTEGER GENERATED ALWAYS AS IDENTITY,
                       email VARCHAR(255) NOT NULL,
                       password_hash VARCHAR(255) NOT NULL,
                       phone VARCHAR(20),
                       role user_role NOT NULL,
                       created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                       updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                       is_email_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
                       confirmed_at TIMESTAMPTZ,
                       PRIMARY KEY (id)
                   ) PARTITION BY HASH (id);
                   """;

        await db.Database.ExecuteSqlRawAsync(sql).ConfigureAwait(false);

        for (var i = 0; i < PartitionCount; i++)
        {
            var partitionSql = $"""
                                CREATE TABLE {PartitionedTable}_p{i}
                                PARTITION OF {PartitionedTable}
                                FOR VALUES WITH (MODULUS {PartitionCount}, REMAINDER {i});
                                """;

            await db.Database.ExecuteSqlRawAsync(partitionSql).ConfigureAwait(false);
        }

        return Results.Ok(new
        {
            Message = $"Создана партицированная таблица {PartitionedTable} с {PartitionCount} hash-партициями",
            Partitions = Enumerable.Range(0, PartitionCount).Select(i => $"{PartitionedTable}_p{i}"),
        });
    }

    private static async Task<IResult> SeedDataAsync(ApplicationDbContext db, int count = 50)
    {
        var exists = await db.Database.SqlQueryRaw<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{PartitionedTable}') AS \"Value\"")
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (!exists)
        {
            return Results.BadRequest(new { Message = "Сначала инициализируйте партиции через /init" });
        }

        var roles = new[] { "client", "tutor", "admin", "super_admin" };

        for (var i = 0; i < count; i++)
        {
            var role = roles[i % roles.Length];
            var sql = $"""
                       INSERT INTO {PartitionedTable} (email, password_hash, role)
                       VALUES (
                           'user_{Guid.NewGuid():N}@demo.tutoflow.ru',
                           'hash_{Guid.NewGuid():N}',
                           '{role}'::user_role
                       );
                       """;

            await db.Database.ExecuteSqlRawAsync(sql).ConfigureAwait(false);
        }

        return Results.Ok(new SeedResult(count, $"Добавлено {count} пользователей в партицированную таблицу"));
    }

    private static async Task<IResult> AddUserAsync(ApplicationDbContext db, string email, string role = "client")
    {
        var exists = await db.Database.SqlQueryRaw<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{PartitionedTable}') AS \"Value\"")
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (!exists)
        {
            return Results.BadRequest(new { Message = "Сначала инициализируйте партиции через /init" });
        }

        var sql = $"""
                   INSERT INTO {PartitionedTable} (email, password_hash, role)
                   VALUES (
                       '{email}',
                       'hash_{Guid.NewGuid():N}',
                       '{role}'::user_role
                   )
                   RETURNING id
                   """;

        var conn = db.Database.GetDbConnection();
        await conn.OpenAsync().ConfigureAwait(false);
        var cmd = conn.CreateCommand();
        await using var _ = cmd.ConfigureAwait(false);
#pragma warning disable CA2100
        cmd.CommandText = sql;
#pragma warning restore CA2100
        var id = (int)(await cmd.ExecuteScalarAsync().ConfigureAwait(false))!;

        return Results.Ok(new { Id = id, Email = email, Role = role, Message = $"Пользователь добавлен (id={id}), PostgreSQL автоматически определил партицию" });
    }

    private static async Task<IResult> GetPartitionStatsAsync(ApplicationDbContext db)
    {
        var exists = await db.Database.SqlQueryRaw<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{PartitionedTable}') AS \"Value\"")
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (!exists)
        {
            return Results.Ok(new { Message = "Партицированная таблица не создана", Stats = Array.Empty<PartitionStats>() });
        }

        var stats = await db.Database.SqlQueryRaw<PartitionStats>("""
                                                                  SELECT
                                                                      child.relname AS "PartitionName",
                                                                      child.reltuples::bigint AS "RowCount"
                                                                  FROM pg_inherits
                                                                  JOIN pg_class parent ON pg_inherits.inhparent = parent.oid
                                                                  JOIN pg_class child ON pg_inherits.inhrelid = child.oid
                                                                  WHERE parent.relname = 'users_partitioned'
                                                                  ORDER BY child.relname
                                                                  """)
            .ToListAsync()
            .ConfigureAwait(false);

        var exactStats = new List<PartitionStats>();

        foreach (var stat in stats)
        {
            var countResult = await db.Database.SqlQueryRaw<long>($"SELECT COUNT(*)::bigint AS \"Value\" FROM {stat.PartitionName}")
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            exactStats.Add(stat with
            {
                RowCount = countResult,
            });
        }

        var totalCount = exactStats.Sum(s => s.RowCount);
        return Results.Ok(new { TotalRows = totalCount, Stats = exactStats });
    }

    private static async Task<IResult> ResetAsync(ApplicationDbContext db)
    {
        var exists = await db.Database.SqlQueryRaw<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = '{PartitionedTable}') AS \"Value\"")
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (!exists)
        {
            return Results.Ok(new { Message = "Партицированная таблица не найдена" });
        }

        await db.Database.ExecuteSqlRawAsync($"DROP TABLE IF EXISTS {PartitionedTable} CASCADE")
            .ConfigureAwait(false);

        return Results.Ok(new { Message = "Партицированная таблица и все партиции удалены" });
    }
}
