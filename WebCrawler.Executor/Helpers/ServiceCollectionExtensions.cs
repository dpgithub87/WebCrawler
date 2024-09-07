using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebCrawler.Domain.Helpers;
using WebCrawler.Domain.Services;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Domain.Validators;
using WebCrawler.Executor.Config;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services;
using WebCrawler.Executor.Services.CrawlResultsHandler;
using WebCrawler.Executor.Services.Interfaces;
using WebCrawler.Infrastructure.Clients;
using WebCrawler.Infrastructure.Clients.Interfaces;
using WebCrawler.Infrastructure.Config;
using WebCrawler.Infrastructure.Repository;
using WebCrawler.Infrastructure.Repository.Interfaces;

namespace WebCrawler.Executor.Helpers;

public static class ServiceCollectionExtensions
{
    public static void ConfigureCrawlerServices(this IServiceCollection services, HostBuilderContext context)
    {
        ConfigureThreadSafeCollections(services);
        ConfigureWebPageDownloader(services);
        ConfigureDownloadedContentHandlers(services);
        ConfigureCrawlResultsHandler(services);
        
        services.AddHostedService<CrawlerBackgroundService>();
        services.Configure<CrawlOptions>(context.Configuration.GetSection("CrawlOptions"));
        services.Configure<InfrastructureOptions>(context.Configuration.GetSection("InfrastructureOptions"));
        
        services.AddDistributedMemoryCache();
    }
    private static void ConfigureThreadSafeCollections(IServiceCollection services)
    {
        services.AddSingleton<ConcurrentDictionary<Uri, bool>>();
        services.AddSingleton<BlockingCollection<CrawlTask>>();
    }
    
    private static void ConfigureWebPageDownloader(IServiceCollection services)
    {
        services.AddHttpClient<IWebContentDownloaderClient, WebContentDownloaderClient>();
        services.AddScoped<IWebContentDownloaderService, WebContentDownloaderService>();
        services.AddScoped<IWebContentRepository, WebContentRepository>();
        services.AddScoped<IDistributedCacheWrapper, DistributedCacheWrapper>();
    }
    
    private static void ConfigureDownloadedContentHandlers(IServiceCollection services)
    {
        services.AddScoped<IUriValidator, UriValidator>();
        services.AddScoped<IUriExtractorService, UriExtractorService>();
        services.AddScoped<IHtmlParser, HtmlAgilityParser>();
        services.AddScoped<IDownloadedContentHandlerFactory, DownloadedContentHandlerFactory>();
        services.AddScoped<IDownloadedContentHandler, HtmlContentHandler>();
        services.AddScoped<IUriProcessorService, UriProcessorService>();
    }
    
    private static void ConfigureCrawlResultsHandler(IServiceCollection services)
    {
        services.AddScoped<ICrawlResultsHandler, JsonCrawlResultsHandler>();
        services.AddScoped<ICrawlResultsHandler, CsvCrawlResultsHandler>();
        services.AddScoped<ICrawlResultsHandlerFactory, CrawlResultsHandlerFactory>();
    }
    
}