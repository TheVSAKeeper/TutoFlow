#pragma warning disable CA1515
using System.Text.Encodings.Web;
using System.Text.Json;

namespace TutoFlow.Web.Components.Pages;

public partial class Partitioning
{
    private static readonly JsonSerializerOptions IndentedJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private bool _isLoading;
    private string? _lastResult;
    private string? _errorMessage;
    private int _seedCount = 50;
    private string _addEmail = string.Empty;
    private string _addRole = "client";

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
