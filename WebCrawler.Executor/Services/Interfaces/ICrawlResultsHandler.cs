using WebCrawler.Executor.Models;

namespace WebCrawler.Executor.Services.Interfaces;

public interface ICrawlResultsHandler
{
    Task WriteResults(string filePath, List<CrawlResult> crawlResults);
}