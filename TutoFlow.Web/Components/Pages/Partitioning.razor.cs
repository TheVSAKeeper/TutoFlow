#pragma warning disable CA1515, CA5394
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TutoFlow.Web.Components.Pages;

public partial class Partitioning
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
    private int _seedCount = 50;
    private string _addEmail = string.Empty;
    private string _addRole = "client";
    private PartitionDataResponse? _shardData;

    private static readonly Random Rng = new();
    private static readonly string[] Roles = ["client", "tutor", "admin", "super_admin"];

    private void FillRandomUser()
    {
        _addEmail = $"user_{Guid.NewGuid().ToString("N")[..8]}@demo.tutoflow.ru";
        _addRole = Roles[Rng.Next(Roles.Length)];
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
        return ExecuteAsync(() => ShardingApi.InitPartitioningAsync());
    }

    private Task SeedAsync()
    {
        return ExecuteAsync(() => ShardingApi.SeedPartitioningAsync(_seedCount));
    }

    private Task AddUserAsync()
    {
        return ExecuteAsync(() => ShardingApi.AddPartitionedUserAsync(_addEmail, _addRole));
    }

    private Task GetStatsAsync()
    {
        return ExecuteAsync(() => ShardingApi.GetPartitionStatsAsync());
    }

    private Task ResetAsync()
    {
        return ExecuteAsync(() => ShardingApi.ResetPartitioningAsync());
    }

    private async Task LoadShardDataAsync()
    {
        try
        {
            var raw = await ShardingApi.GetPartitionDataAsync().ConfigureAwait(false);
            _shardData = JsonSerializer.Deserialize<PartitionDataResponse>(raw, CamelCaseOptions);
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

    private sealed class PartitionDataResponse
    {
        [JsonPropertyName("totalRows")]
        public int TotalRows { get; set; }

        [JsonPropertyName("partitions")]
        public List<PartitionGroup> Partitions { get; set; } = [];
    }

    private sealed class PartitionGroup
    {
        [JsonPropertyName("partitionName")]
        public string PartitionName { get; set; } = string.Empty;

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("users")]
        public List<PartitionUser> Users { get; set; } = [];
    }

    private sealed class PartitionUser
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("partitionName")]
        public string PartitionName { get; set; } = string.Empty;
    }
}
