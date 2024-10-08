using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Executor.Config;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor.Services
{
    public class UriProcessorService : IUriProcessorService
    {
        private readonly IWebContentDownloaderService _webDownloader;
        private readonly ConcurrentDictionary<Uri, bool> _processedUris;
        private readonly BlockingCollection<CrawlTask> _tasks;
        private readonly ILogger<UriProcessorService> _logger;
        private readonly ICrawlResultsHandlerFactory _crawlResultsHandlerFactory;
        private readonly CrawlOptions _crawlOptions;
        private readonly IWebContentHandlerFactory _contentHandlerFactory;


        public UriProcessorService(IWebContentDownloaderService webDownloader,
            ConcurrentDictionary<Uri, bool> processedUris,
            BlockingCollection<CrawlTask> tasks,
            ILogger<UriProcessorService> logger,
            ICrawlResultsHandlerFactory crawlResultsHandlerFactory,
            IWebContentHandlerFactory contentHandlerFactory,
            IOptions<CrawlOptions> crawlSettings)
        {
            _webDownloader = webDownloader;
            _processedUris = processedUris;
            _tasks = tasks;
            _logger = logger;
            _crawlResultsHandlerFactory = crawlResultsHandlerFactory;
            _contentHandlerFactory = contentHandlerFactory;
            _crawlOptions = crawlSettings.Value;
        }

        public async Task ProcessUri(CrawlTask task, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                task.Status = CrawlTaskStatus.Processing;

                var (success, links) = await DownloadWebContentAndHandleBasedOnType(task, cancellationToken);

                if (!success) return;

                var scheduler = AddBackgroundTasksToCrawlLinksInBfsOrder(task, cancellationToken, links!);
                var crawlResultTask = BuildAndWriteResults(task, links!, stopwatch);
                await Task.WhenAll(scheduler, crawlResultTask);

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

        private async Task<(bool success, List<Uri>? links)> DownloadWebContentAndHandleBasedOnType(CrawlTask task, CancellationToken cancellationToken)
        {
            var webContent = await _webDownloader.DownloadContent(task.Uri, cancellationToken);
            if (webContent == null)
            {
                task.Status = CrawlTaskStatus.Failed;
                return (false, null);
            }

            var contentType = webContent.ContentType;
            var handler = _contentHandlerFactory.CreateHandler(contentType!);
            return await handler.HandleContentAsync(webContent, task.Uri);
        }

        private async Task AddBackgroundTasksToCrawlLinksInBfsOrder(CrawlTask parentTask, CancellationToken cancellationToken, List<Uri> links)
        {
            _processedUris.TryAdd(parentTask.Uri, true);

            await Parallel.ForEachAsync(links, cancellationToken, (link, token) =>
            {
                if (parentTask.Uri.Host != link.Host)
                {
                    _logger.LogInformation($"Skipping link with different host: {link.Host}");
                    return ValueTask.CompletedTask;
                }
                _logger.LogInformation(link.ToString());
                if (_processedUris.TryAdd(link, true))
                {
                    var newTask = new CrawlTask(link, parentTask.OutputFilePath, parentTask.Uri, parentTask.DepthLevel + 1);
                    _tasks.Add(newTask, token);
                }

                return ValueTask.CompletedTask;
            });
        }

        private async Task BuildAndWriteResults(CrawlTask task, List<Uri> links, Stopwatch stopwatch)
        {
            var crawlResult = new CrawlResult(task.Uri, task.ParentUri)
            {
                Links = links,
                CrawlTime = stopwatch.Elapsed,
                DepthLevel = task.DepthLevel
            };
            var resultsHandler = _crawlResultsHandlerFactory.GetHandler(_crawlOptions.OutputFormat!);
            await resultsHandler?.WriteResults(task.OutputFilePath, new List<CrawlResult> { crawlResult })!;
        }
    }
}