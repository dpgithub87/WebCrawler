using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Infrastructure.Clients.Interfaces;

public interface IWebContentDownloaderClient
{
    Task<WebContent?> DownloadAsync(Uri targetUri);

}