using CommandLine;

namespace WebCrawler.Executor.Config;

public class CrawlOptions
{
    [Option('u', "url", HelpText = "Set the initial crawl URIs as a comma-separated string.")]
    public string? InitialCrawlUris { get; set; }

    public string? OutputFilePath { get; set; }

    [Option('d', "maxdepth", HelpText = "Set the maximum crawl depth.")]
    public int? MaxDepth { get; set; }

    [Option('f', "format", HelpText = "Set the output format.")]
    public string? OutputFormat { get; set; }
}