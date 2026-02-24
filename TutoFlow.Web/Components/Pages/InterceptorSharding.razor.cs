using System.Text.Encodings.Web;
using System.Text.Json;

namespace TutoFlow.Web.Components.Pages;

public partial class InterceptorSharding
{
    private static readonly JsonSerializerOptions IndentedJsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    private bool _isLoading;
    private string? _lastResult;
    private string? _errorMessage;
    private int _seedCount = 30;
    private int _queryClientProfileId = 1;

    private static string FormatJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, IndentedJsonOptions);
        }
        catch
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

    private async Task ExecuteAsync(Func<Task<string>> action)
    {
        _isLoading = true;
        _errorMessage = null;
        _lastResult = null;

        try
        {
            var raw = await action();
            _lastResult = FormatJson(raw);
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isLoading = false;
        }
    }
}
