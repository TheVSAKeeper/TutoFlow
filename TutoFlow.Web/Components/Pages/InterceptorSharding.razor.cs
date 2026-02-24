#pragma warning disable CA1515
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
    private string _addFullName = string.Empty;
    private int _addClientProfileId = 1;
    private short _addGrade = 5;

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
