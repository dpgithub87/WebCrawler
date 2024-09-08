using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Config;
using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Infrastructure.Clients;

public class WebContentDownloaderClient : IWebContentDownloaderClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WebContentDownloaderClient> _logger;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public WebContentDownloaderClient(HttpClient httpClient, ILogger<WebContentDownloaderClient> logger, IOptions<InfrastructureOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "CrawlerBot/1.0");

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

    public async Task<WebContent?> DownloadAsync(Uri targetUri)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(targetUri));

            if (HandleFailureCases(targetUri, response)) return null;

            var webContent = new WebContent()
            {
                Content = await response.Content.ReadAsStringAsync(),
                ContentType = response.Content.Headers.ContentType?.MediaType
            };

            return webContent;
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
        return false;
    }
}
