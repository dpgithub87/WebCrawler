using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCrawler.Executor.Helpers;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.UriProcessor;

namespace WebCrawler.Executor
{
    public class CrawlerBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly BlockingCollection<CrawlTask> _tasks;
        private readonly ConcurrentDictionary<Uri, bool> _processedUris;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CrawlerBackgroundService> _logger;
        private readonly IUriProcessor _uriProcessor;

        public CrawlerBackgroundService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<CrawlerBackgroundService> logger
            , IUriProcessor uriProcessor, BlockingCollection<CrawlTask> tasks, ConcurrentDictionary<Uri, bool> processedUris)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
            _uriProcessor = uriProcessor;
            _tasks = tasks;
            _processedUris = processedUris;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var (initialCrawlUris, outPutFilePath, maxDepth) = GetCrawlUrisAndSettings();

            if (initialCrawlUris == null || initialCrawlUris.Count == 0)
                return;

            // Convert the valid links to a collection of tasks
            foreach (var uri in initialCrawlUris)
            {
                _tasks.Add(new CrawlTask(uri,outPutFilePath), stoppingToken);
            }

            // Monitor the _tasks collection and process tasks
            while (!stoppingToken.IsCancellationRequested)
            {
                if (!_tasks.TryTake(out var task, Timeout.Infinite, stoppingToken)) continue;

                if (task.Level > maxDepth)
                {
                    task.Status = CrawlTaskStatus.Failed;
                    continue;
                }
                
                if (task.Status == CrawlTaskStatus.Pending)
                {
                    _ = Task.Run(() => _uriProcessor.ProcessUri(task, stoppingToken), stoppingToken);
                }
            }
        }

        private (List<Uri>?, string, int) GetCrawlUrisAndSettings()
        {
            var initialCrawlUris = _configuration.GetSection("InitialCrawlUris").Get<List<string>>()?
                .Select(uri => new Uri(uri))
                .ToList();

            var projectRoot = OutputPathHelper.GetProjectRoot();
            var relativePath = _configuration["CrawlSettings:OutputFilePath"]!;
            var outputFilePath = Path.Combine(projectRoot, relativePath);

            var maxDepth = _configuration.GetValue<int>("CrawlSettings:MaxDepth");
            
            return (initialCrawlUris, outputFilePath, maxDepth);
        }
    }
}