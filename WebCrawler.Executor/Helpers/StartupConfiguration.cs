using CommandLine;
using Microsoft.Extensions.Configuration;
using WebCrawler.Executor.Config;

namespace WebCrawler.Executor.Helpers;

public static class StartupConfiguration
{
    public static void Setup(IConfigurationBuilder config, string[] args)
    {
        config.SetBasePath(AppContext.BaseDirectory);
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Parse command-line arguments and update configuration
        var configuration = config.Build();
        var settings = configuration.GetSection("CrawlOptions").Get<CrawlOptions>();

        Parser.Default.ParseArguments<CrawlOptions>(args)
            .WithParsed(parsedSettings =>
            {
                settings = MergeSettings(settings!, parsedSettings);
            });

        // Update the configuration with the merged settings
        var inMemorySettings = new Dictionary<string, string>
        {
            { "CrawlOptions:InitialCrawlUris", settings.InitialCrawlUris },
            { "CrawlOptions:OutputFilePath", settings.OutputFilePath },
            { "CrawlOptions:MaxDepth", settings.MaxDepth.ToString() },
            { "CrawlOptions:OutputFormat", settings.OutputFormat }
        };

        config.AddInMemoryCollection(inMemorySettings);
    }

    private static CrawlOptions MergeSettings(CrawlOptions baseOptions, CrawlOptions overrideOptions)
    {
        if (!string.IsNullOrEmpty(overrideOptions.InitialCrawlUris))
        {
            baseOptions.InitialCrawlUris = overrideOptions.InitialCrawlUris;
        }

        if (!string.IsNullOrEmpty(overrideOptions.OutputFilePath))
        {
            baseOptions.OutputFilePath = overrideOptions.OutputFilePath;
        }

        if (overrideOptions.MaxDepth.HasValue)
        {
            baseOptions.MaxDepth = overrideOptions.MaxDepth.Value;
        }

        if (!string.IsNullOrEmpty(overrideOptions.OutputFormat))
        {
            baseOptions.OutputFormat = overrideOptions.OutputFormat;
        }

        return baseOptions;
    }
}