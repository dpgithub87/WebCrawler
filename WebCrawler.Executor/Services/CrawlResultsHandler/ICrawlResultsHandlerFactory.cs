using WebCrawler.Executor.Services.Interfaces;

namespace WebCrawler.Executor.Services.CrawlResultsHandler;

public interface ICrawlResultsHandlerFactory
{
    ICrawlResultsHandler? GetHandler(string handlerType);
}