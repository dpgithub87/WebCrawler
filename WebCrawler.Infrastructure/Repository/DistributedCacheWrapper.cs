using Microsoft.Extensions.Caching.Distributed;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Infrastructure.Repository;

public class DistributedCacheWrapper : IDistributedCacheWrapper
{
    private readonly IDistributedCache _distributedCache;

    public DistributedCacheWrapper(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public Task<string?> GetStringAsync(string key, CancellationToken token)
    {
        return _distributedCache.GetStringAsync(key, token);
    }

    public Task SetStringAsync(string key, string value, DistributedCacheEntryOptions options, CancellationToken token)
    {
        return _distributedCache.SetStringAsync(key, value, options, token);
    }
}