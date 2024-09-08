using System.Text.Json;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor.Services.CrawlResultsHandler
{
    public class JsonCrawlResultsHandler : ICrawlResultsHandler
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        public async Task WriteResults(string outputFilePath, List<CrawlResult> newResults)
        {
            await Semaphore.WaitAsync();
            try
            {
                List<CrawlResult> existingResults;

                // Ensure the directory exists
                var directory = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                if (File.Exists(outputFilePath))
                {
                    var existingJson = await File.ReadAllTextAsync(outputFilePath);
                    existingResults = JsonSerializer.Deserialize<List<CrawlResult>>(existingJson) ??
                                      new List<CrawlResult>();
                }
                else
                {
                    existingResults = new List<CrawlResult>();
                }

                existingResults.AddRange(newResults);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(existingResults, options);
                await File.WriteAllTextAsync(outputFilePath, json);
            }
            finally
            {
                Semaphore.Release();
            }


        }
    }
}