using Microsoft.Extensions.Logging;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Config;
using WebCrawler.Infrastructure.Models;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Infrastructure.Repository;

public class WebContentRepository : IWebContentRepository
{
    private readonly IWebContentDownloaderClient _webContentDownloaderClient;
    private readonly IDistributedCacheWrapper _cache;
    private readonly ILogger<WebContentRepository> _logger;

    public WebContentRepository(IWebContentDownloaderClient webContentDownloaderClient, IDistributedCacheWrapper cache, ILogger<WebContentRepository> logger)
    {
        _webContentDownloaderClient = webContentDownloaderClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<WebContent?> GetWebPageAsync(Uri targetUri, CancellationToken cancellationToken)
    {
        var cacheKey = $"WebPage_{targetUri.AbsoluteUri}";
        var cachedContent = await _cache.GetWebContentAsync(cacheKey, cancellationToken);
        
        if (cachedContent != null)
        {
            _logger.LogInformation($"Cache hit for {targetUri}");
            return cachedContent;
        }
        
        var content = await _webContentDownloaderClient.DownloadAsync(targetUri);
        if (content != null)
        {
            await _cache.SetWebContentAsync(cacheKey, content, cancellationToken);
        }

        return content;
    }
}