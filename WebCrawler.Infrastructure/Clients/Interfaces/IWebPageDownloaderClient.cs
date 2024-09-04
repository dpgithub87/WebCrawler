namespace WebCrawler.Infrastructure.Clients.Interfaces;

public interface IWebPageDownloaderClient
{    
    Task<string?> DownloadPageAsync(Uri targetUri);
    
}