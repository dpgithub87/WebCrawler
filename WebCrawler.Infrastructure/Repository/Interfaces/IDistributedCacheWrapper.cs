using Microsoft.Extensions.Caching.Distributed;
using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Infrastructure.Repository.Interfaces;

public interface IDistributedCacheWrapper
{
    Task<WebContent?> GetDownloadedContentAsync(string key, CancellationToken cancellationToken);
    Task SetDownloadedContentAsync(string key, WebContent data, CancellationToken cancellationToken);
}