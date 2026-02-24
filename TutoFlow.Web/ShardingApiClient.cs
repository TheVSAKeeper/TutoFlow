#pragma warning disable MA0048, CA1515
namespace TutoFlow.Web;

internal sealed class ShardingApiClient(HttpClient httpClient)
{
    public async Task<string> InitPartitioningAsync(CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync(new Uri("/api/sharding/partitioning/init", UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> SeedPartitioningAsync(int count = 50, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync(new Uri($"/api/sharding/partitioning/seed?count={count}", UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> AddPartitionedUserAsync(string email, string role, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync(new Uri($"/api/sharding/partitioning/add?email={Uri.EscapeDataString(email)}&role={Uri.EscapeDataString(role)}", UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public Task<string> GetPartitionStatsAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/partitioning/stats", UriKind.Relative), ct);
    }

    public Task<string> GetPartitionDataAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/partitioning/data", UriKind.Relative), ct);
    }

    public async Task<string> ResetPartitioningAsync(CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync(new Uri("/api/sharding/partitioning/reset", UriKind.Relative), ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> InitAppLevelAsync(CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync(new Uri("/api/sharding/app-level/init", UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> SeedAppLevelAsync(int count = 20, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync(new Uri($"/api/sharding/app-level/seed?count={count}", UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> AddAppLevelCenterAsync(string name, string? address, CancellationToken ct = default)
    {
        var uri = $"/api/sharding/app-level/add?name={Uri.EscapeDataString(name)}";
        if (!string.IsNullOrEmpty(address))
        {
            uri += $"&address={Uri.EscapeDataString(address)}";
        }

        var response = await httpClient.PostAsync(new Uri(uri, UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public Task<string> GetAppLevelStatsAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/app-level/stats", UriKind.Relative), ct);
    }

    public Task<string> GetAppLevelDataAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/app-level/data", UriKind.Relative), ct);
    }

    public async Task<string> QueryAppLevelCenterAsync(string name, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(new Uri($"/api/sharding/app-level/query?name={Uri.EscapeDataString(name)}", UriKind.Relative), ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> ResetAppLevelAsync(CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync(new Uri("/api/sharding/app-level/reset", UriKind.Relative), ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> InitInterceptorAsync(CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync(new Uri("/api/sharding/interceptor/init", UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> SeedInterceptorAsync(int count = 30, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsync(new Uri($"/api/sharding/interceptor/seed?count={count}", UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> AddInterceptorStudentAsync(string fullName, int clientProfileId, short? grade, CancellationToken ct = default)
    {
        var uri = $"/api/sharding/interceptor/add?fullName={Uri.EscapeDataString(fullName)}&clientProfileId={clientProfileId}";
        if (grade.HasValue)
        {
            uri += $"&grade={grade.Value}";
        }

        var response = await httpClient.PostAsync(new Uri(uri, UriKind.Relative), null, ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public Task<string> GetInterceptorStatsAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/interceptor/stats", UriKind.Relative), ct);
    }

    public Task<string> GetInterceptorDataAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/interceptor/data", UriKind.Relative), ct);
    }

    public async Task<string> QueryInterceptorStudentAsync(int clientProfileId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(new Uri($"/api/sharding/interceptor/query?clientProfileId={clientProfileId}", UriKind.Relative), ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public async Task<string> ResetInterceptorAsync(CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync(new Uri("/api/sharding/interceptor/reset", UriKind.Relative), ct).ConfigureAwait(false);
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }
}
