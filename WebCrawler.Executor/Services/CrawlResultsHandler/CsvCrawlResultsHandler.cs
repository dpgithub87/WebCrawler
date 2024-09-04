using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor.Services.CrawlResultsHandler;

public class CsvCrawlResultsHandler : ICrawlResultsHandler
{
    private static readonly SemaphoreSlim Semaphore = new (1, 1);

    public async Task WriteResults(string filePath, List<CrawlResult> crawlResults)
    {
        await Semaphore.WaitAsync();
        try
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = !File.Exists(filePath)
            };

            await using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            await using var writer = new StreamWriter(stream);
            await using var csv = new CsvWriter(writer, config);
            await csv.WriteRecordsAsync(crawlResults);
        }
        finally
        {
            Semaphore.Release();
        }
      
    }
}