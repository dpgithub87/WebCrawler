using Microsoft.Extensions.Caching.Distributed;

namespace WebCrawler.Infrastructure.Repository.Interfaces;

public interface IDistributedCacheWrapper
{
    Task<string?> GetStringAsync(string key, CancellationToken token);
    Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken token);
}