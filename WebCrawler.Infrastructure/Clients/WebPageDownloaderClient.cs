using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Config;

namespace WebCrawler.Infrastructure.Clients;

public class WebPageDownloaderClient : IWebPageDownloaderClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebPageDownloaderClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public WebPageDownloaderClient(HttpClient httpClient, ILogger<WebPageDownloaderClient> logger, IOptions<InfrastructureOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Other");

        // Define a Polly retry policy
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(options.Value.PollyRetryCount!.Value, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (result, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning($"Request failed with {result.Result?.StatusCode}. Waiting {timeSpan} before next retry. Retry attempt {retryCount}");
                });
    }

    public async Task<string?> DownloadPageAsync(Uri targetUri)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(targetUri));

            if (HandleFailureCases(targetUri, response)) return null;

            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred while downloading page: {ex.Message}");
            return null;
        }
    }

    private bool HandleFailureCases(Uri targetUri, HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Failed to download page. Status code: {response.StatusCode}");
            return true;
        }

        var contentType = response.Content.Headers.ContentType;
        if (contentType == null || !contentType.MediaType!.Contains("text/html"))
        {
            _logger.LogInformation(
                $"Content in url:{targetUri} has content type:{contentType}. This is non-html. Ignoring!");
            return true;
        }

        return false;
    }
}
