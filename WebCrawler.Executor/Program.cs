using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCrawler.Executor;
using WebCrawler.Executor.Config;
using WebCrawler.Executor.Extensions;

var host = CreateHostBuilder(args).Build();

await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(AppContext.BaseDirectory);
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            
            // Parse command-line arguments and update configuration
            var configuration = config.Build();
            var settings = configuration.GetSection("CrawlSettings").Get<CrawlSettings>();

            Parser.Default.ParseArguments<CrawlSettings>(args)
                .WithParsed(parsedSettings =>
                {
                    settings = MergeSettings(settings, parsedSettings);
                });

            // Update the configuration with the merged settings
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                { "CrawlSettings:InitialCrawlUris", settings.InitialCrawlUris },
                { "CrawlSettings:MaxDepth", settings.MaxDepth.ToString() },
                { "CrawlSettings:OutputFormat", settings.OutputFormat }
            });
            
        })
        .ConfigureServices((context, services) =>
        {
            services.ConfigureThreadSafeCollections();

            services.ConfigureWebPageDownloader();
            
            services.ConfigureUriExtractor();
            
            services.ConfigureCrawlResultsHandler();
            
            services.AddHostedService<CrawlerBackgroundService>();
            
            var settings = context.Configuration.GetSection("CrawlSettings").Get<CrawlSettings>();
            
            services.Configure<CrawlSettings>(context.Configuration.GetSection("CrawlSettings"));
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        
static CrawlSettings MergeSettings(CrawlSettings baseSettings, CrawlSettings overrideSettings)
{
    if (overrideSettings.InitialCrawlUris != null && overrideSettings.InitialCrawlUris.Any())
    {
        baseSettings.InitialCrawlUris = overrideSettings.InitialCrawlUris;
    }
    
    if (overrideSettings.MaxDepth.HasValue)
    {
        baseSettings.MaxDepth = overrideSettings.MaxDepth.Value;
    }

    if (!string.IsNullOrEmpty(overrideSettings.OutputFormat))
    {
        baseSettings.OutputFormat = overrideSettings.OutputFormat;
    }

    return baseSettings;
}