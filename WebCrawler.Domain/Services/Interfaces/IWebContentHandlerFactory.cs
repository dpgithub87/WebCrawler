namespace WebCrawler.Domain.Services.Interfaces;

public interface IWebContentHandlerFactory
{
    IWebContentHandler CreateHandler(string contentType);
}