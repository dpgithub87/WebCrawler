using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using WebCrawler.Domain.Helpers;
using WebCrawler.Domain.Services;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Domain.Validators;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.Services;
using WebCrawler.Executor.Services.CrawlResultsHandler;
using WebCrawler.Executor.Services.Interfaces;
using WebCrawler.Infrastructure.Clients;
using WebCrawler.Infrastructure.Clients.Interfaces;

namespace WebCrawler.Executor.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureThreadSafeCollections(this IServiceCollection services)
    {
        services.AddSingleton<ConcurrentDictionary<Uri, bool>>();
        services.AddSingleton<BlockingCollection<CrawlTask>>();
    }
    
    public static void ConfigureWebPageDownloader(this IServiceCollection services)
    {
        services.AddHttpClient<IWebPageDownloaderClient, WebPageDownloaderClient>();
        services.AddScoped<IWebPageDownloaderService, WebPageDownloaderService>();
    }
    
    public static void ConfigureUriExtractor(this IServiceCollection services)
    {
        services.AddScoped<IUriValidator, UriValidator>();
        services.AddScoped<IUriProcessorService, UriProcessorService>();
        services.AddScoped<IUriExtractorService, UriExtractorService>();
        services.AddScoped<IHtmlParser, HtmlAgilityParser>();
    }
    
    public static void ConfigureCrawlResultsHandler(this IServiceCollection services)
    {
        services.AddScoped<ICrawlResultsHandler, JsonCrawlResultsHandler>();
        services.AddScoped<ICrawlResultsHandler, CsvCrawlResultsHandler>();
        services.AddScoped<ICrawlResultsHandlerFactory, CrawlResultsHandlerFactory>();
    }
    
}