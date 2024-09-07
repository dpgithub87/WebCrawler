namespace WebCrawler.Executor.Services.Interfaces;

public interface ICrawlResultsHandlerFactory
{
    ICrawlResultsHandler? GetHandler(string handlerType);
}