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

    public Task<string> GetPartitionStatsAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/partitioning/stats", UriKind.Relative), ct);
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

    public Task<string> GetAppLevelStatsAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/app-level/stats", UriKind.Relative), ct);
    }

    public async Task<string> QueryAppLevelCenterAsync(int centerId, CancellationToken ct = default)
    {
        var response = await httpClient.GetAsync(new Uri($"/api/sharding/app-level/query?centerId={centerId}", UriKind.Relative), ct).ConfigureAwait(false);
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

    public Task<string> GetInterceptorStatsAsync(CancellationToken ct = default)
    {
        return httpClient.GetStringAsync(new Uri("/api/sharding/interceptor/stats", UriKind.Relative), ct);
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
