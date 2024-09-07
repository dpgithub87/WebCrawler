using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Config;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Infrastructure.Repository;

public class WebPageRepository : IWebPageRepository
{
    private readonly IWebPageDownloaderClient _webPageDownloaderClient;
    private readonly IDistributedCacheWrapper _cache;
    private readonly ILogger<WebPageRepository> _logger;
    private readonly InfrastructureOptions _options;

    public WebPageRepository(IWebPageDownloaderClient webPageDownloaderClient, IDistributedCacheWrapper cache, ILogger<WebPageRepository> logger, IOptions<InfrastructureOptions> options)
    {
        _webPageDownloaderClient = webPageDownloaderClient;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<string?> GetWebPageAsync(Uri targetUri, CancellationToken cancellationToken)
    {
        var cacheKey = $"WebPage_{targetUri.AbsoluteUri}";
        var cachedContent = await _cache.GetStringAsync(cacheKey, cancellationToken);
        
        if (!string.IsNullOrEmpty(cachedContent))
        {
            _logger.LogInformation($"Cache hit for {targetUri}");
            return cachedContent;
        }
        
        var content = await _webPageDownloaderClient.DownloadPageAsync(targetUri);
        if (content != null)
        {
            await _cache.SetStringAsync(cacheKey, content, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.CacheExpirySeconds!.Value)
            }, cancellationToken);
        }

        return content;
    }
}