// See https://aka.ms/new-console-template for more information

using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCrawler.Domain.Helpers;
using WebCrawler.Domain.Services;
using WebCrawler.Domain.Services.Interfaces;
using WebCrawler.Domain.Validators;
using WebCrawler.Executor;
using WebCrawler.Executor.Models;
using WebCrawler.Executor.ResultsFormatter;
using WebCrawler.Executor.UriProcessor;
using WebCrawler.Infrastructure.Clients;
using WebCrawler.Infrastructure.Clients.Interfaces;

var host = CreateHostBuilder(args).Build();
await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(AppContext.BaseDirectory);
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
        .ConfigureServices((context, services) =>
        {
            services.AddSingleton<ConcurrentDictionary<Uri, bool>>();
            services.AddSingleton<BlockingCollection<CrawlTask>>();
            
            services.AddHttpClient<IWebPageDownloaderClient, WebPageDownloaderClient>();
            services.AddScoped<IWebPageDownloaderService, WebPageDownloaderService>();
            
            services.AddScoped<IHtmlParser, HtmlAgilityParser>();
            services.AddScoped<IUriExtractorService, UriExtractorService>();
            
            services.AddScoped<IUriValidator, UriValidator>();
            services.AddScoped<IUriProcessor, UriProcessor>();
            
            services.AddScoped<ICrawlerResultsFormatter, JsonResultsFormatter>();
            
            services.AddHostedService<CrawlerBackgroundService>();
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });