using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor.Services.CrawlResultsHandler;

public class CrawlResultsHandlerFactory : ICrawlResultsHandlerFactory
{
    private readonly IEnumerable<ICrawlResultsHandler> _handlers;

    public CrawlResultsHandlerFactory(IEnumerable<ICrawlResultsHandler> handlers)
    {
        _handlers = handlers;
    }

    public ICrawlResultsHandler? GetHandler(string handlerType)
    {
        return handlerType switch
        {
            "csv" => _handlers.OfType<CsvCrawlResultsHandler>().FirstOrDefault(),
            _ => _handlers.OfType<JsonCrawlResultsHandler>().FirstOrDefault()
        };
    }
}