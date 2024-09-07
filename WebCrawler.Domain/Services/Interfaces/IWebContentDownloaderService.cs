using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Domain.Services.Interfaces;

public interface IWebContentDownloaderService
{
    public Task<WebContent?> DownloadContent(Uri targetUri, CancellationToken cancellationToken);
}