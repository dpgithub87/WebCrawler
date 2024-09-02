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
        private readonly CrawlSettings _crawlSettings;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CrawlerBackgroundService> _logger;
        private readonly IUriProcessorService _uriProcessorService;

        public CrawlerBackgroundService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<CrawlerBackgroundService> logger
            , IUriProcessorService uriProcessorService, BlockingCollection<CrawlTask> tasks, ConcurrentDictionary<Uri, bool> processedUris, IOptions<CrawlSettings> crawlSettings)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
            _uriProcessorService = uriProcessorService;
            _tasks = tasks;
            _processedUris = processedUris;
            _crawlSettings = crawlSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var initialCrawlUris = _crawlSettings.InitialCrawlUris?.Split(",").ToList();
            var absoluteOutputFilePath = GetAbsoluteOutputPathFromConfig();
            
            if (initialCrawlUris == null || initialCrawlUris.Count == 0)
                return;

            // Convert the valid links to a collection of tasks
            foreach (var strUri in initialCrawlUris)
            {
                var uri = Uri.TryCreate(strUri, UriKind.Absolute, out var result) ? result : null;
                if (uri == null)
                {
                    _logger.LogWarning($"Invalid URI: {strUri}");
                    continue;
                }
                _tasks.Add(new CrawlTask(uri, absoluteOutputFilePath), stoppingToken);
            }

            MonitorAndProcessTasks(stoppingToken);
        }

        private void MonitorAndProcessTasks(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_tasks.TryTake(out var task, Timeout.Infinite, stoppingToken)) continue;

                if (task.Level > _crawlSettings.MaxDepth)
                {
                    task.Status = CrawlTaskStatus.Failed;
                    continue;
                }

                if (task.Status == CrawlTaskStatus.Pending)
                {
                    _ = Task.Run(() => _uriProcessorService.ProcessUri(task, stoppingToken), stoppingToken);
                }
            }
        }

        private string GetAbsoluteOutputPathFromConfig()
        {
       
            var projectRoot = OutputPathHelper.GetProjectRoot();
            var outputFileName = $"crawl_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid()}.json";
            var relativePath = $"{_crawlSettings.OutputFilePath!}/{outputFileName}";
            var absOutputFilePath = Path.Combine(projectRoot, relativePath);
            return absOutputFilePath;
        }
    }
}