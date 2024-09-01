using WebCrawler.Executor.Models;

namespace WebCrawler.Executor.UriProcessor;

public interface IUriProcessor
{
    Task ProcessUri(CrawlTask task, CancellationToken stoppingToken);
}
