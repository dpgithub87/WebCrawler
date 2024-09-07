using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Domain.Services.Interfaces;

public interface IWebContentDownloaderService
{
    public Task<DownloadedContent?> DownloadPage(Uri targetUri, CancellationToken cancellationToken);
}