using WebCrawler.Executor.Models;

namespace WebCrawler.Executor.Services.Interfaces;

public interface IUriProcessorService
{
    Task ProcessUri(CrawlTask task, CancellationToken stoppingToken);
}
