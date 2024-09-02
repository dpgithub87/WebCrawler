using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Executor.Config;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services.CrawlResultsHandler;
using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor.Services
{
    public class UriProcessorService : IUriProcessorService
    {
        private readonly IWebPageDownloaderService _webDownloader;
        private readonly IUriExtractorService _uriExtractor;
        private readonly ConcurrentDictionary<Uri, bool> _processedUris;
        private readonly BlockingCollection<CrawlTask> _tasks;
        private readonly ILogger<UriProcessorService> _logger;
        private readonly ICrawlResultsHandlerFactory _crawlResultsHandlerFactory;
        private readonly CrawlSettings _crawlSettings;

        public UriProcessorService(
            IWebPageDownloaderService webDownloader,
            IUriExtractorService uriExtractor,
            ConcurrentDictionary<Uri, bool> processedUris,
            BlockingCollection<CrawlTask> tasks,
            ILogger<UriProcessorService> logger,
            ICrawlResultsHandlerFactory crawlResultsHandlerFactory, IOptions<CrawlSettings> crawlSettings)
        {
            _webDownloader = webDownloader;
            _uriExtractor = uriExtractor;
            _processedUris = processedUris;
            _tasks = tasks;
            _logger = logger;
            _crawlResultsHandlerFactory = crawlResultsHandlerFactory;
            _crawlSettings = crawlSettings.Value;
        }

        public async Task ProcessUri(CrawlTask task, CancellationToken stoppingToken)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                task.Status = CrawlTaskStatus.Processing;

                var outputFormat = _crawlSettings.OutputFormat;
                
                var (htmlContent, links) = await DownloadAndExtractUris(task.Uri);
                if (htmlContent == null)
                {
                    task.Status = CrawlTaskStatus.Failed;
                    return;
                }

                _processedUris.TryAdd(task.Uri, true);
                
                AddBackgroundTasksToCrawlLinksInBfsOrder(task, stoppingToken, links);

                BuildAndWriteResults(task, links, outputFormat, stopwatch);

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
        
        private void BuildAndWriteResults(CrawlTask task, IEnumerable<Uri> links, string outputFormat, Stopwatch stopwatch)
        {
            var crawlResult = new CrawlResult(task.Uri, task.ParentUri)
            {
                Links = links.ToList(),
                CrawlTime = stopwatch.Elapsed,
                Level = task.Level
            };
            var resultsHandler = _crawlResultsHandlerFactory.GetHandler(outputFormat);
            resultsHandler?.WriteResults(task.OutputFilePath, new List<CrawlResult> { crawlResult });
        }
    }
}