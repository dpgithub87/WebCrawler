using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Infrastructure.Clients.Interfaces;

public interface IWebContentDownloaderClient
{    
    Task<DownloadedContent?> DownloadPageAsync(Uri targetUri);
    
}