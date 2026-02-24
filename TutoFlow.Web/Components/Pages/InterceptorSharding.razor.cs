#pragma warning disable CA1515, CA5394
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TutoFlow.Web.Components.Pages;

public partial class InterceptorSharding
{
    private static readonly JsonSerializerOptions IndentedJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private bool _isLoading;
    private string? _lastResult;
    private string? _errorMessage;
    private int _seedCount = 30;
    private int _queryClientProfileId = 1;
    private string _addFullName = string.Empty;
    private int _addClientProfileId = 1;
    private short _addGrade = 5;
    private SchemaDataResponse? _shardData;

    private static readonly Random Rng = new();

    private static readonly string[] StudentNames =
    [
        "Иванов Иван", "Петрова Мария", "Сидоров Алексей", "Козлова Анна",
        "Михайлов Дмитрий", "Новикова Елена", "Фёдоров Сергей", "Морозова Ольга",
        "Волков Артём", "Лебедева Дарья",
    ];

    private void FillRandomStudent()
    {
        _addFullName = StudentNames[Rng.Next(StudentNames.Length)];
        _addClientProfileId = Rng.Next(1, 200);
        _addGrade = (short)Rng.Next(1, 12);
    }

    private static string FormatJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, IndentedJsonOptions);
        }
        catch (JsonException)
        {
            return json;
        }
    }

    private Task InitAsync()
    {
        return ExecuteAsync(() => ShardingApi.InitInterceptorAsync());
    }

    private Task SeedAsync()
    {
        return ExecuteAsync(() => ShardingApi.SeedInterceptorAsync(_seedCount));
    }

    private Task AddStudentAsync()
    {
        return ExecuteAsync(() => ShardingApi.AddInterceptorStudentAsync(_addFullName, _addClientProfileId, _addGrade));
    }

    private Task GetStatsAsync()
    {
        return ExecuteAsync(() => ShardingApi.GetInterceptorStatsAsync());
    }

    private Task QueryStudentAsync()
    {
        return ExecuteAsync(() => ShardingApi.QueryInterceptorStudentAsync(_queryClientProfileId));
    }

    private Task ResetAsync()
    {
        return ExecuteAsync(() => ShardingApi.ResetInterceptorAsync());
    }

    private async Task LoadShardDataAsync()
    {
        try
        {
            var raw = await ShardingApi.GetInterceptorDataAsync().ConfigureAwait(false);
            _shardData = JsonSerializer.Deserialize<SchemaDataResponse>(raw, CamelCaseOptions);
        }
        catch (HttpRequestException)
        {
            _shardData = null;
        }
    }

    private async Task ExecuteAsync(Func<Task<string>> action)
    {
        _isLoading = true;
        _errorMessage = null;
        _lastResult = null;

        try
        {
            var raw = await action().ConfigureAwait(false);
            _lastResult = FormatJson(raw);
            await LoadShardDataAsync().ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private sealed class SchemaDataResponse
    {
        [JsonPropertyName("totalStudents")]
        public int TotalStudents { get; set; }

        [JsonPropertyName("schemas")]
        public List<SchemaGroup> Schemas { get; set; } = [];
    }

    private sealed class SchemaGroup
    {
        [JsonPropertyName("schemaName")]
        public string SchemaName { get; set; } = string.Empty;

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("students")]
        public List<StudentInfo> Students { get; set; } = [];
    }

    private sealed class StudentInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [JsonPropertyName("grade")]
        public short? Grade { get; set; }

        [JsonPropertyName("clientProfileId")]
        public int ClientProfileId { get; set; }

        [JsonPropertyName("schemaName")]
        public string SchemaName { get; set; } = string.Empty;
    }
}
