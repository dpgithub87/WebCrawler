using WebCrawler.Infrastructure.Models;

namespace WebCrawler.Infrastructure.Repository.Interfaces;

public interface IWebPageRepository
{
    Task<DownloadedContent?> GetWebPageAsync(Uri targetUri, CancellationToken cancellationToken);
}