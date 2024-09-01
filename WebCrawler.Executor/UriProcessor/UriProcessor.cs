using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.ResultsFormatter;

namespace WebCrawler.Executor.UriProcessor
{
    public class UriProcessor : IUriProcessor
    {
        private readonly IWebPageDownloaderService _webDownloader;
        private readonly IUriExtractorService _uriExtractor;
        private readonly ConcurrentDictionary<Uri, bool> _processedUris;
        private readonly BlockingCollection<CrawlTask> _tasks;
        private readonly ILogger<UriProcessor> _logger;
        private readonly ICrawlerResultsFormatter _resultsFormatter;

        public UriProcessor(
            IWebPageDownloaderService webDownloader,
            IUriExtractorService uriExtractor,
            ConcurrentDictionary<Uri, bool> processedUris,
            BlockingCollection<CrawlTask> tasks,
            ILogger<UriProcessor> logger,
            ICrawlerResultsFormatter resultsFormatter)
        {
            _webDownloader = webDownloader;
            _uriExtractor = uriExtractor;
            _processedUris = processedUris;
            _tasks = tasks;
            _logger = logger;
            _resultsFormatter = resultsFormatter;
        }

        public async Task ProcessUri(CrawlTask task, CancellationToken stoppingToken)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                task.Status = CrawlTaskStatus.Processing;

                var (htmlContent, links) = await DownloadAndExtractUris(task.Uri);
                if (htmlContent == null)
                {
                    task.Status = CrawlTaskStatus.Failed;
                    return;
                }

                _processedUris.TryAdd(task.Uri, true);
                
                AddBackgroundTasksToCrawlLinksInBfsOrder(task, stoppingToken, links);

                BuildAndWriteResults(task, links, stopwatch);

                task.Status = CrawlTaskStatus.Completed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing URI: {task.Uri}");
                task.Status = CrawlTaskStatus.Failed;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private async Task<(string? htmlContent, IEnumerable<Uri> links)> DownloadAndExtractUris(Uri uri)
        {
            var htmlContent = await _webDownloader.DownloadPage(uri);
            if (htmlContent == null)
            {
                return (null, Enumerable.Empty<Uri>());
            }

            var links = _uriExtractor.ExtractValidUrls(htmlContent, uri);
            return (htmlContent, links);
        }

        private void AddBackgroundTasksToCrawlLinksInBfsOrder(CrawlTask parentTask, CancellationToken stoppingToken, IEnumerable<Uri> links)
        {
            foreach (var link in links)
            {
                _logger.LogInformation(link.ToString());
                if (_processedUris.TryAdd(link, true))
                {
                    var newTask = new CrawlTask(link, parentTask.OutputFilePath, parentTask.Uri, parentTask.Level + 1);
                    _tasks.Add(newTask, stoppingToken);
                }
            }
        }
        
        private void BuildAndWriteResults(CrawlTask task, IEnumerable<Uri> links, Stopwatch stopwatch)
        {
            var crawlResult = new CrawlResult(task.Uri, task.ParentUri)
            {
                Links = links.ToList(),
                CrawlTime = stopwatch.Elapsed,
                Level = task.Level
            };

            _resultsFormatter.WriteResults(task.OutputFilePath, new List<CrawlResult> { crawlResult });
        }
    }
}