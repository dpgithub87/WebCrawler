namespace WebCrawler.Infrastructure.Config;

public class InfrastructureOptions
{
    public int? CacheExpirySeconds { get; set; }
    public int? PollyRetryCount { get; set; }
}