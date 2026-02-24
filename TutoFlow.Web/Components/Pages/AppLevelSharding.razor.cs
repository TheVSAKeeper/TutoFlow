#pragma warning disable CA1515
using System.Text.Encodings.Web;
using System.Text.Json;

namespace TutoFlow.Web.Components.Pages;

public partial class AppLevelSharding
{
    private static readonly JsonSerializerOptions IndentedJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private bool _isLoading;
    private string? _lastResult;
    private string? _errorMessage;
    private int _seedCount = 20;
    private string _queryCenterName = string.Empty;
    private string _addName = string.Empty;
    private string _addAddress = string.Empty;

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

    private async Task ExecuteAsync(Func<Task<string>> action)
    {
        _isLoading = true;
        _errorMessage = null;
        _lastResult = null;

        try
        {
            var raw = await action().ConfigureAwait(false);
            _lastResult = FormatJson(raw);
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
}
