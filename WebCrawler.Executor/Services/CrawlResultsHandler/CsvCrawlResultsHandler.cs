using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor.Services.CrawlResultsHandler;

public class CsvCrawlResultsHandler : ICrawlResultsHandler
{
    private static readonly object Lock = new object();// Lock object for thread safety

    public void WriteResults(string filePath, List<CrawlResult> crawlResults)
    {
        lock (Lock)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = !File.Exists(filePath)
            };

            using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            using var writer = new StreamWriter(stream);
            using var csv = new CsvWriter(writer, config);
            csv.WriteRecords(crawlResults);
        }
    }
}