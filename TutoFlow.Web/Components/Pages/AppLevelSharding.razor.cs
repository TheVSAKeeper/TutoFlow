#pragma warning disable CA1515
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TutoFlow.Web.Components.Pages;

public partial class AppLevelSharding
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
    private int _seedCount = 20;
    private string _queryCenterName = string.Empty;
    private string _addName = string.Empty;
    private string _addAddress = string.Empty;
    private ShardDataResponse? _shardData;

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
        return ExecuteAsync(() => ShardingApi.InitAppLevelAsync());
    }

    private Task SeedAsync()
    {
        return ExecuteAsync(() => ShardingApi.SeedAppLevelAsync(_seedCount));
    }

    private Task AddCenterAsync()
    {
        return ExecuteAsync(() => ShardingApi.AddAppLevelCenterAsync(_addName, _addAddress));
    }

    private Task GetStatsAsync()
    {
        return ExecuteAsync(() => ShardingApi.GetAppLevelStatsAsync());
    }

    private Task QueryCenterAsync()
    {
        return ExecuteAsync(() => ShardingApi.QueryAppLevelCenterAsync(_queryCenterName));
    }

    private Task ResetAsync()
    {
        return ExecuteAsync(() => ShardingApi.ResetAppLevelAsync());
    }

    private async Task LoadShardDataAsync()
    {
        try
        {
            var raw = await ShardingApi.GetAppLevelDataAsync().ConfigureAwait(false);
            _shardData = JsonSerializer.Deserialize<ShardDataResponse>(raw, CamelCaseOptions);
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

    private sealed class ShardDataResponse
    {
        [JsonPropertyName("totalCenters")]
        public int TotalCenters { get; set; }

        [JsonPropertyName("shards")]
        public List<ShardGroup> Shards { get; set; } = [];
    }

    private sealed class ShardGroup
    {
        [JsonPropertyName("shardName")]
        public string ShardName { get; } = string.Empty;

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }

        [JsonPropertyName("centers")]
        public List<CenterInfo> Centers { get; } = [];
    }

    private sealed class CenterInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; } = string.Empty;

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("shardName")]
        public string ShardName { get; set; } = string.Empty;
    }
}
