using Microsoft.Extensions.Caching.Distributed;
using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Infrastructure.Repository.Interfaces;

public interface IDistributedCacheWrapper
{
    Task<WebContent?> GetWebContentAsync(string key, CancellationToken cancellationToken);
    Task SetWebContentAsync(string key, WebContent data, CancellationToken cancellationToken);
}