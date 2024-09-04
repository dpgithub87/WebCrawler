using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebCrawler.Executor.Utilities;

var host = CreateHostBuilder(args).Build();

await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((context, config) =>
        {
            StartupConfiguration.Setup(config, args);
        })
        .ConfigureServices((context, services) =>
        {
            services.ConfigureCrawlerServices(context);
        })
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });
        