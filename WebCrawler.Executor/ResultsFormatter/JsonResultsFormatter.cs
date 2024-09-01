using System.Text.Json;
using WebCrawler.Executor.Models;

namespace WebCrawler.Executor.ResultsFormatter
{
    public class JsonResultsFormatter : ICrawlerResultsFormatter
    {
        // Lock object for thread safety
        private static readonly object Lock = new object();

        public void WriteResults(string outputFilePath, List<CrawlResult> newResults)
        {
            lock (Lock)
            {
                List<CrawlResult> existingResults;
                
                if (File.Exists(outputFilePath))
                {
                    var existingJson = File.ReadAllText(outputFilePath);
                    existingResults = JsonSerializer.Deserialize<List<CrawlResult>>(existingJson) ?? new List<CrawlResult>();
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
                File.WriteAllText(outputFilePath, json);
               
                // using var stream = new FileStream(outputFilePath, FileMode.Append, FileAccess.Write, FileShare.None);
                // using var writer = new StreamWriter(stream);
                // foreach (var result in newResults)
                // {
                //     var json = JsonSerializer.Serialize(result);
                //     writer.WriteLine(json);
                // }
                
            }
        }
    }
}