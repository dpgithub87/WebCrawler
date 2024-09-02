using WebCrawler.Executor.Models;

namespace WebCrawler.Executor.Services.Interfaces;

public interface ICrawlResultsHandler
{
    void WriteResults(string filePath, List<CrawlResult> crawlResults);
}