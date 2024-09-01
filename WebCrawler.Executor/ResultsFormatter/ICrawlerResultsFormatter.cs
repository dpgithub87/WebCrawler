using WebCrawler.Executor.Models;

namespace WebCrawler.Executor.ResultsFormatter;

public interface ICrawlerResultsFormatter
{
    void WriteResults(string filePath, List<CrawlResult> crawlResults);
}