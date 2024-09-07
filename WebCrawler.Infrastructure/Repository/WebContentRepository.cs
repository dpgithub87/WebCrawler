using Microsoft.Extensions.Logging;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Config;
using WebCrawler.Infrastructure.Models;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Infrastructure.Repository;

public class WebPageRepository : IWebPageRepository
{
    private readonly IWebPageDownloaderClient _webPageDownloaderClient;
    private readonly IDistributedCacheWrapper _cache;
    private readonly ILogger<WebPageRepository> _logger;

    public WebPageRepository(IWebPageDownloaderClient webPageDownloaderClient, IDistributedCacheWrapper cache, ILogger<WebPageRepository> logger)
    {
        _webPageDownloaderClient = webPageDownloaderClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DownloadedContent?> GetWebPageAsync(Uri targetUri, CancellationToken cancellationToken)
    {
        var cacheKey = $"WebPage_{targetUri.AbsoluteUri}";
        var cachedContent = await _cache.GetDownloadedContentAsync(cacheKey, cancellationToken);
        
        if (cachedContent != null)
        {
            _logger.LogInformation($"Cache hit for {targetUri}");
            return cachedContent;
        }
        
        var content = await _webPageDownloaderClient.DownloadPageAsync(targetUri);
        if (content != null)
        {
            await _cache.SetDownloadedContentAsync(cacheKey, content, cancellationToken);
        }

        return content;
    }
}