using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebCrawler.Executor.Config;
using WebCrawler.Executor.Helpers;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor
{
    public class CrawlerBackgroundService : BackgroundService
    {
        private readonly BlockingCollection<CrawlTask> _crawlTasks;
        private readonly CrawlOptions _crawlOptions;
        private readonly ILogger<CrawlerBackgroundService> _logger;
        private readonly IUriProcessorService _uriProcessorService;
        private readonly ConcurrentBag<Task> _runningTasks = new();
        private readonly IHostApplicationLifetime _applicationLifetime;

        public CrawlerBackgroundService(IUriProcessorService uriProcessorService, BlockingCollection<CrawlTask> crawlTasks,
            IOptions<CrawlOptions> crawlOptions, IHostApplicationLifetime applicationLifetime, ILogger<CrawlerBackgroundService> logger)
        {
            _uriProcessorService = uriProcessorService;
            _crawlTasks = crawlTasks;
            _applicationLifetime = applicationLifetime;
            _crawlOptions = crawlOptions.Value;
            _applicationLifetime = applicationLifetime;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!GetConfigValues(out var initialCrawlUris, out var absoluteOutputFilePath)) return;

                CreateTasksForInitialUris(cancellationToken, initialCrawlUris, absoluteOutputFilePath);

                await MonitorAndProcessCrawlTasks(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing crawl tasks.");
            }
        }

        private bool GetConfigValues(out List<string>? initialCrawlUris, out string absoluteOutputFilePath)
        {
            initialCrawlUris = _crawlOptions.InitialCrawlUris?.Split(",").ToList();
            absoluteOutputFilePath = GetAbsoluteOutputPathFromConfig();

            return initialCrawlUris != null && initialCrawlUris.Count != 0;
        }

        private void CreateTasksForInitialUris(CancellationToken cancellationToken, List<string>? initialCrawlUris,
            string absoluteOutputFilePath)
        {
            foreach (var strUri in initialCrawlUris!)
            {
                var uri = Uri.TryCreate(strUri, UriKind.Absolute, out var result) ? result : null;
                if (uri == null)
                {
                    _logger.LogWarning($"Invalid URI: {strUri}");
                    continue;
                }

                _crawlTasks.Add(new CrawlTask(uri, absoluteOutputFilePath), cancellationToken);
            }
        }

        private async Task MonitorAndProcessCrawlTasks(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_crawlTasks.TryTake(out var task, 10000, cancellationToken))
                {
                    _logger.LogInformation("No new tasks received for 10 seconds. Shutting down background service.");
                    break;
                }

                if (task.DepthLevel > _crawlOptions.MaxDepth)
                {
                    task.Status = CrawlTaskStatus.LimitReached;
                    continue;
                }

                await AddDelayForPoliteness(cancellationToken);

                if (task.Status != CrawlTaskStatus.Pending) continue;

                var processingTask = Task.Run(() => _uriProcessorService.ProcessUri(task, cancellationToken), cancellationToken);
                _runningTasks.Add(processingTask);
            }

            await Task.WhenAll(_runningTasks.ToArray());
            _logger.LogInformation("All tasks completed. Shutting down application.");
            _applicationLifetime.StopApplication();
        }

        private string GetAbsoluteOutputPathFromConfig()
        {
            var projectRoot = OutputPathHelper.GetProjectRoot();
            var outputFormat = _crawlOptions.OutputFormat;
            var outputFileName = $"crawl_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}.{outputFormat}";
            var relativePath = $"{_crawlOptions.OutputFilePath!}/{outputFileName}";
            var absOutputFilePath = Path.Combine(projectRoot, relativePath);
            return absOutputFilePath;
        }
        private static async Task AddDelayForPoliteness(CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
        }
    }
}