using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using WebCrawler.Infrastructure.Config;
using WebCrawler.Infrastructure.Models;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Infrastructure.Repository;

public class DistributedCacheWrapper : IDistributedCacheWrapper
{
    private readonly IDistributedCache _distributedCache;
    private readonly InfrastructureOptions _options;

    public DistributedCacheWrapper(IDistributedCache distributedCache, IOptions<InfrastructureOptions> options)
    {
        _distributedCache = distributedCache;
        _options = options.Value;
    }

    public async Task<WebContent?> GetWebContentAsync(string key, CancellationToken cancellationToken)
    {
        var cachedBytes = await _distributedCache.GetAsync(key, cancellationToken);
        return cachedBytes == null ? null : JsonSerializer.Deserialize<WebContent>(cachedBytes);
    }

    public async Task SetWebContentAsync(string key, WebContent data, CancellationToken cancellationToken)
    {
        var dataBytes = JsonSerializer.SerializeToUtf8Bytes(data);
      
        await _distributedCache.SetAsync(key, dataBytes, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.CacheExpirySeconds!.Value)
        }, cancellationToken);
    }
}