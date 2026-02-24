using Microsoft.Extensions.Logging;

namespace TutoFlow.Tests;

internal sealed class WebTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Test]
    public async Task GetWebResourceRootReturnsOkStatusCodeAsync()
    {
        // Arrange
        var cancellationToken = TestContext.CurrentContext.CancellationToken;

        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.TutoFlow_AppHost>(cancellationToken).ConfigureAwait(false);
        appHost.Services.AddLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Debug);
            // Override the logging filters from the app's configuration
            logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
            logging.AddFilter("Aspire.", LogLevel.Debug);
        });

        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });

#pragma warning disable CA2007
#pragma warning disable MA0004
        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken).ConfigureAwait(false);
#pragma warning restore MA0004
#pragma warning restore CA2007
        await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken).ConfigureAwait(false);

        // Act
        using var httpClient = app.CreateHttpClient("webfrontend");
        await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken).ConfigureAwait(false);
        var response = await httpClient.GetAsync(new Uri("/", UriKind.Relative), cancellationToken).ConfigureAwait(false);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
