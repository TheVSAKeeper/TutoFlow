#pragma warning disable MA0048, MA0051, CA1515, EF1002, CA2100
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using TutoFlow.ApiService.Data;
using TutoFlow.ApiService.Data.Models;

namespace TutoFlow.ApiService.Sharding.InterceptorSharding;

internal static class ShardContext
{
    public static readonly string[] AllShards = ["shard_a", "shard_b"];
    private static readonly AsyncLocal<string?> CurrentShard = new();

    public static string? Current
    {
        get => CurrentShard.Value;
        set => CurrentShard.Value = value;
    }

    public static string Resolve(int clientProfileId)
    {
        return clientProfileId % 2 == 0 ? AllShards[0] : AllShards[1];
    }
}

internal sealed class ShardRoutingInterceptor : DbCommandInterceptor
{
    private const string DefaultSchema = "public";
    private const string TargetTable = "students";

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        RewriteCommand(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        RewriteCommand(command);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        RewriteCommand(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        RewriteCommand(command);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        RewriteCommand(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        RewriteCommand(command);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    private static void RewriteCommand(DbCommand command)
    {
        var shard = ShardContext.Current;
        if (string.IsNullOrEmpty(shard))
        {
            return;
        }

        command.CommandText = command.CommandText
            .Replace($"\"{DefaultSchema}\".\"{TargetTable}\"", $"\"{shard}\".\"{TargetTable}\"", StringComparison.Ordinal)
            .Replace($"{DefaultSchema}.{TargetTable}", $"{shard}.{TargetTable}", StringComparison.Ordinal);
    }
}

internal sealed class InterceptorShardingDbContext(DbContextOptions<InterceptorShardingDbContext> options)
    : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("students", "public");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").UseIdentityAlwaysColumn();
            entity.Property(e => e.ClientProfileId).HasColumnName("client_profile_id").IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Grade).HasColumnName("grade");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.IsSelf).HasColumnName("is_self").HasDefaultValue(false);
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("NOW()");

            entity.Ignore(e => e.ClientProfile);
        });

        modelBuilder.Ignore<ClientProfile>();
        modelBuilder.Ignore<Center>();
        modelBuilder.Ignore<User>();
        modelBuilder.Ignore<TutorProfile>();
        modelBuilder.Ignore<AdminProfile>();
        modelBuilder.Ignore<CenterMembership>();
    }
}

internal sealed record SchemaShardStats(string SchemaName, int RowCount, string[] StudentNames);

internal sealed record ShardStudentInfo(int Id, string FullName, short? Grade, int ClientProfileId, string SchemaName);

internal sealed record SchemaDataGroup(string SchemaName, int RowCount, ShardStudentInfo[] Students);

internal static class InterceptorShardingDemo
{
    public static void MapInterceptorShardingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/sharding/interceptor");

        group.MapPost("/init", InitAsync)
            .WithName("InitInterceptorSharding")
            .WithDescription("Создаёт схемы shard_a и shard_b с таблицей students");

        group.MapPost("/seed", SeedAsync)
            .WithName("SeedInterceptorSharding")
            .WithDescription("Заполняет студентами, маршрутизируя по client_profile_id");

        group.MapPost("/add", AddStudentAsync)
            .WithName("AddInterceptorStudent")
            .WithDescription("Добавляет одного студента с маршрутизацией по client_profile_id");

        group.MapGet("/stats", GetStatsAsync)
            .WithName("GetInterceptorStats")
            .WithDescription("Возвращает количество строк в каждой схеме");

        group.MapGet("/data", GetDataAsync)
            .WithName("GetInterceptorData")
            .WithDescription("Возвращает студентов каждой схемы с полной информацией");

        group.MapGet("/query", QueryStudentAsync)
            .WithName("QueryInterceptorStudent")
            .WithDescription("Находит студентов по client_profile_id на нужном шарде");

        group.MapDelete("/reset", ResetAsync)
            .WithName("ResetInterceptorSharding")
            .WithDescription("Удаляет схемы шардов");
    }

    private static async Task<IResult> InitAsync(ApplicationDbContext db)
    {
        foreach (var schema in ShardContext.AllShards)
        {
            await db.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS {schema}").ConfigureAwait(false);

            await db.Database.ExecuteSqlRawAsync($"""
                                                  CREATE TABLE IF NOT EXISTS {schema}.students (
                                                      id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
                                                      client_profile_id INTEGER NOT NULL,
                                                      full_name VARCHAR(255) NOT NULL,
                                                      grade SMALLINT,
                                                      birth_date DATE,
                                                      is_self BOOLEAN NOT NULL DEFAULT FALSE,
                                                      notes TEXT,
                                                      created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                                      updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                                                      CONSTRAINT chk_students_grade CHECK (grade BETWEEN 1 AND 12)
                                                  )
                                                  """)
                .ConfigureAwait(false);
        }

        return Results.Ok(new
        {
            Message = "Схемы и таблицы инициализированы",
            Schemas = ShardContext.AllShards,
        });
    }

    private static async Task<IResult> AddStudentAsync(InterceptorShardingDbContext db, string fullName, int clientProfileId, short? grade = null)
    {
        var shard = ShardContext.Resolve(clientProfileId);
        ShardContext.Current = shard;

        var student = new Student
        {
            FullName = fullName,
            Grade = grade,
            ClientProfileId = clientProfileId,
        };

        db.Students.Add(student);
        await db.SaveChangesAsync().ConfigureAwait(false);

        ShardContext.Current = null;

        return Results.Ok(new ShardStudentInfo(student.Id, student.FullName, student.Grade, student.ClientProfileId, shard));
    }

    private static async Task<IResult> SeedAsync(InterceptorShardingDbContext db, int count = 30)
    {
        var names = new[]
        {
            "Иванов Иван", "Петрова Мария", "Сидоров Алексей", "Козлова Анна",
            "Михайлов Дмитрий", "Новикова Елена", "Фёдоров Сергей", "Морозова Ольга",
            "Волков Артём", "Лебедева Дарья",
        };

        var created = new List<ShardStudentInfo>();

        for (var i = 1; i <= count; i++)
        {
            var clientProfileId = i;
            var shard = ShardContext.Resolve(clientProfileId);

            ShardContext.Current = shard;

            var student = new Student
            {
                FullName = $"{names[i % names.Length]} #{i}",
                Grade = (short)(1 + i % 11),
                ClientProfileId = clientProfileId,
            };

            db.Students.Add(student);
            await db.SaveChangesAsync().ConfigureAwait(false);

            created.Add(new(student.Id, student.FullName, student.Grade, student.ClientProfileId, shard));

            db.Entry(student).State = EntityState.Detached;
        }

        ShardContext.Current = null;

        return Results.Ok(new { InsertedCount = created.Count, Students = created });
    }

    private static async Task<IResult> GetStatsAsync(ApplicationDbContext db)
    {
        var stats = new List<SchemaShardStats>();

        foreach (var schema in ShardContext.AllShards)
        {
            var schemaExists = await db.Database.SqlQueryRaw<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = '{schema}') AS \"Value\"")
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (!schemaExists)
            {
                stats.Add(new(schema, 0, []));
                continue;
            }

            var count = await db.Database.SqlQueryRaw<long>($"SELECT COUNT(*)::bigint AS \"Value\" FROM {schema}.students")
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            var names = await db.Database.SqlQueryRaw<string>($"SELECT full_name AS \"Value\" FROM {schema}.students ORDER BY id")
                .ToArrayAsync()
                .ConfigureAwait(false);

            stats.Add(new(schema, (int)count, names));
        }

        return Results.Ok(new { TotalStudents = stats.Sum(s => s.RowCount), Stats = stats });
    }

    private static async Task<IResult> GetDataAsync(ApplicationDbContext db)
    {
        var groups = new List<SchemaDataGroup>();

        foreach (var schema in ShardContext.AllShards)
        {
            var schemaExists = await db.Database.SqlQueryRaw<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = '{schema}') AS \"Value\"")
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (!schemaExists)
            {
                groups.Add(new(schema, 0, []));
                continue;
            }

            var students = await db.Database.SqlQueryRaw<ShardStudentInfo>($"""
                                                                            SELECT
                                                                                id AS "Id",
                                                                                full_name AS "FullName",
                                                                                grade::smallint AS "Grade",
                                                                                client_profile_id AS "ClientProfileId",
                                                                                '{schema}' AS "SchemaName"
                                                                            FROM {schema}.students
                                                                            ORDER BY id
                                                                            """)
                .ToArrayAsync()
                .ConfigureAwait(false);

            groups.Add(new(schema, students.Length, students));
        }

        return Results.Ok(new { TotalStudents = groups.Sum(g => g.RowCount), Schemas = groups });
    }

    private static async Task<IResult> QueryStudentAsync(InterceptorShardingDbContext db, int clientProfileId)
    {
        var shard = ShardContext.Resolve(clientProfileId);
        ShardContext.Current = shard;

        var students = await db.Students
            .Where(s => s.ClientProfileId == clientProfileId)
            .ToListAsync()
            .ConfigureAwait(false);

        ShardContext.Current = null;

        if (students.Count == 0)
        {
            return Results.NotFound(new { Message = $"Студенты с clientProfileId={clientProfileId} не найдены на шарде {shard}" });
        }

        var result = students.Select(s => new ShardStudentInfo(s.Id, s.FullName, s.Grade, s.ClientProfileId, shard));
        return Results.Ok(result);
    }

    private static async Task<IResult> ResetAsync(ApplicationDbContext db)
    {
        foreach (var schema in ShardContext.AllShards)
        {
            await db.Database.ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS {schema} CASCADE")
                .ConfigureAwait(false);
        }

        return Results.Ok(new { Message = "Схемы шардов удалены" });
    }
}
