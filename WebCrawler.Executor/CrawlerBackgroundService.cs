using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockingCollection<CrawlTask> _tasks;
        private readonly ConcurrentDictionary<Uri, bool> _processedUris;
        private readonly CrawlOptions _crawlOptions;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CrawlerBackgroundService> _logger;
        private readonly IUriProcessorService _uriProcessorService;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public CrawlerBackgroundService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<CrawlerBackgroundService> logger
            ,IUriProcessorService uriProcessorService, BlockingCollection<CrawlTask> tasks, ConcurrentDictionary<Uri, bool> processedUris, IOptions<CrawlOptions> crawlOptions, IHostApplicationLifetime applicationLifetime)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
            _uriProcessorService = uriProcessorService;
            _tasks = tasks;
            _processedUris = processedUris;
            _applicationLifetime = applicationLifetime;
            _crawlOptions = crawlOptions.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!GetConfigValues(out var initialCrawlUris, out var absoluteOutputFilePath)) return;

                CreateTasksForInitialUris(cancellationToken, initialCrawlUris, absoluteOutputFilePath);

                MonitorAndProcessCrawlTasks(cancellationToken);
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
            // Convert the valid links to a collection of tasks
            foreach (var strUri in initialCrawlUris!)
            {
                var uri = Uri.TryCreate(strUri, UriKind.Absolute, out var result) ? result : null;
                if (uri == null)
                {
                    _logger.LogWarning($"Invalid URI: {strUri}");
                    continue;
                }

                _tasks.Add(new CrawlTask(uri, absoluteOutputFilePath), cancellationToken);
            }
        }

        private void MonitorAndProcessCrawlTasks(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (!_tasks.TryTake(out var task, Timeout.Infinite, cancellationToken)) continue;
                
                if (task.DepthLevel > _crawlOptions.MaxDepth)
                {
                    task.Status = CrawlTaskStatus.LimitReached;
                    continue;
                }

                if (task.Status == CrawlTaskStatus.Pending)
                {
                    _ = Task.Run(() => _uriProcessorService.ProcessUri(task, cancellationToken), cancellationToken);
                }
            }
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
    }
}