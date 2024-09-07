namespace WebCrawler.Domain.Services.Interfaces;

public interface IDownloadedContentHandlerFactory
{
    IDownloadedContentHandler CreateHandler(string contentType);
}